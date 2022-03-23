using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Spid.Cie.OIDC.AspNetCore.Events;
using Spid.Cie.OIDC.AspNetCore.Helpers;
using Spid.Cie.OIDC.AspNetCore.Models;
using Spid.Cie.OIDC.AspNetCore.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Spid.Cie.OIDC.AspNetCore.Extensions;

internal class SpidCieHandler : OpenIdConnectHandler
{
    private OpenIdConnectConfiguration? _configuration;
    private const string NonceProperty = "N";
    private readonly ILogPersister _logPersister;
    private readonly SpidCieEvents _events;
    private readonly IIdentityProvidersHandler _idpHandler;
    private readonly IRelyingPartiesHandler _rpRetriever;
    private readonly ICryptoService _cryptoService;

    public SpidCieHandler(IOptionsMonitor<OpenIdConnectOptions> options,
            ILoggerFactory logger,
            HtmlEncoder htmlEncoder,
            UrlEncoder encoder,
            ISystemClock clock,
            ILogPersister logPersister,
            IIdentityProvidersHandler idpHandler,
            IRelyingPartiesHandler rpRetriever,
            ICryptoService cryptoService,
            SpidCieEvents events)
        : base(options, logger, htmlEncoder, encoder, clock)
    {
        _logPersister = logPersister;
        _events = events;
        _idpHandler = idpHandler;
        _rpRetriever = rpRetriever;
        _cryptoService = cryptoService;
        Events = events;
    }

    protected new SpidCieEvents Events
    {
        get { return _events; }
        set { base.Events = value; }
    }

    protected override Task<object> CreateEventsAsync() => Task.FromResult<object>(_events);

    protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        if (string.IsNullOrEmpty(properties.RedirectUri))
        {
            properties.RedirectUri = OriginalPathBase + OriginalPath + Request.QueryString;
        }

        if (_configuration == null && Options.ConfigurationManager != null)
        {
            _configuration = await Options.ConfigurationManager.GetConfigurationAsync(Context.RequestAborted);
        }

        var message = new OpenIdConnectMessage
        {
            ClientId = Options.ClientId,
            EnableTelemetryParameters = !Options.DisableTelemetry,
            IssuerAddress = _configuration?.AuthorizationEndpoint ?? string.Empty,
            RedirectUri = BuildRedirectUri(Options.CallbackPath),
            Resource = Options.Resource,
            ResponseType = Options.ResponseType,
            Prompt = properties.GetParameter<string>(OpenIdConnectParameterNames.Prompt) ?? Options.Prompt,
            Scope = string.Join(" ", properties.GetParameter<ICollection<string>>(OpenIdConnectParameterNames.Scope) ?? Options.Scope),
        };

        #region Pkce
        var bytes = new byte[32];
        RandomNumberGenerator.Fill(bytes);
        var codeVerifier = Microsoft.AspNetCore.Authentication.Base64UrlTextEncoder.Encode(bytes);

        properties.Items.Add(OAuthConstants.CodeVerifierKey, codeVerifier);

        using SHA256 sha = SHA256.Create();
        var challengeBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier));
        var codeChallenge = WebEncoders.Base64UrlEncode(challengeBytes);

        message.Parameters.Add(OAuthConstants.CodeChallengeKey, codeChallenge);
        message.Parameters.Add(OAuthConstants.CodeChallengeMethodKey, OAuthConstants.CodeChallengeMethodS256);
        #endregion


        message.Nonce = Options.ProtocolValidator.GenerateNonce();
        WriteNonceCookie(message.Nonce);

        GenerateCorrelationId(properties);

        var redirectContext = new RedirectContext(Context, Scheme, Options, properties)
        {
            ProtocolMessage = message
        };

        await Events.RedirectToIdentityProvider(redirectContext);

        message = redirectContext.ProtocolMessage;

        properties.Items[OpenIdConnectDefaults.UserstatePropertiesKey] = message.State;

        properties.Items.Add(OpenIdConnectDefaults.RedirectUriForCodePropertiesKey, message.RedirectUri);

        message.State = Options.StateDataFormat.Protect(properties);

        await Events.PostStateCreated(new PostStateCreatedContext(Context, Scheme, Options, properties)
        {
            ProtocolMessage = message
        });

        Throw<InvalidOperationException>.If(string.IsNullOrEmpty(message.IssuerAddress),
            "Cannot redirect to the authorization endpoint, the configuration may be missing or invalid.");

        var redirectUri = message.CreateAuthenticationRequestUrl();

        Response.Redirect(redirectUri);
        return;
    }

    private void WriteNonceCookie(string nonce)
    {
        Throw<ArgumentNullException>.If(string.IsNullOrEmpty(nonce), nameof(nonce));

        var cookieOptions = Options.NonceCookie.Build(Context, Clock.UtcNow);

        Response.Cookies.Append(Options.NonceCookie.Name + Options.StringDataFormat.Protect(nonce),
            NonceProperty,
            cookieOptions);
    }

    public override async Task SignOutAsync(AuthenticationProperties? properties)
    {
        var accessToken = await Context.GetTokenAsync(Options.SignOutScheme, OpenIdConnectParameterNames.AccessToken);
        if (!string.IsNullOrWhiteSpace(accessToken))
        {
            await RevokeToken(accessToken);
        }

        await Context.SignOutAsync(Options.SignOutScheme);
        Response.Redirect(properties?.RedirectUri ?? Options.SignedOutRedirectUri);
    }

    private async Task RevokeToken(string accessToken)
    {
        var issuer = Context.User.FindFirst(SpidCieConst.Iss)?.Value;
        Throw<InvalidOperationException>.If(string.IsNullOrWhiteSpace(issuer),
            "Current authenticated User doesn't have a 'sub' claim.");

        var idps = await _idpHandler.GetIdentityProviders();
        var idp = idps.FirstOrDefault(i => i.EntityConfiguration.Issuer.Equals(issuer));
        Throw<InvalidOperationException>.If(idp is null,
            $"No IdentityProvider found for the issuer {issuer}");

        var clientId = Context.User.FindFirst(SpidCieConst.Aud)?.Value;
        Throw<InvalidOperationException>.If(string.IsNullOrWhiteSpace(clientId),
            "Current authenticated User doesn't have an 'aud' claim.");

        var rps = await _rpRetriever.GetRelyingParties();
        var rp = rps.FirstOrDefault(r => r.ClientId.Equals(clientId));
        Throw<InvalidOperationException>.If(rp is null,
            $"No RelyingParty found for the clientId {clientId}");

        var keySet = rp!.OpenIdCoreJWKs;
        var key = keySet?.Keys?.FirstOrDefault();
        Throw<InvalidOperationException>.If(key is null,
            $"No key found for the RelyingParty with clientId {clientId}");

        (RSA publicKey, RSA privateKey) = _cryptoService.GetRSAKeys(key!);

        var revocationEndpoint = idp!.EntityConfiguration.Metadata.OpenIdProvider.AdditionalData[SpidCieConst.RevocationEndpoint] as string;
        Throw<InvalidOperationException>.If(string.IsNullOrWhiteSpace(revocationEndpoint),
            $"No RevocationEndpoint specified in the EntityConfiguration of the IdentityProvider {issuer}");

        var request = new TokenRevocationRequest()
        {
            ClientCredentialStyle = ClientCredentialStyle.PostBody,
            Address = revocationEndpoint,
            ClientId = clientId,
            ClientAssertion = new ClientAssertion()
            {
                Type = SpidCieConst.ClientAssertionTypeValue,
                Value = _cryptoService.CreateJWT(publicKey,
                    privateKey,
                    new Dictionary<string, object>() {
                                                { SpidCieConst.Kid, key!.Kid },
                                                { SpidCieConst.Typ, SpidCieConst.TypValue }
                    },
                    new Dictionary<string, object>() {
                                                { SpidCieConst.Iss, clientId! },
                                                { SpidCieConst.Sub, clientId! },
                                                { SpidCieConst.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
                                                { SpidCieConst.Exp, DateTimeOffset.UtcNow.AddMinutes(SpidCieConst.EntityConfigurationExpirationInMinutes).ToUnixTimeSeconds() },
                                                { SpidCieConst.Aud, new string[] { revocationEndpoint! } },
                                                { SpidCieConst.Jti, Guid.NewGuid().ToString() }
                    })
            },
            Token = accessToken
        };

        var responseMessage = await Backchannel.RevokeTokenAsync(request);
        if (responseMessage.HttpStatusCode != System.Net.HttpStatusCode.OK)
        {
            Logger.LogWarning($"AccessToken Revocation returned http status {responseMessage.HttpStatusCode}");
        }
        if (responseMessage.IsError)
        {
            Logger.LogWarning($"AccessToken Revocation returned Error {responseMessage.Error}");
        }
    }
}
