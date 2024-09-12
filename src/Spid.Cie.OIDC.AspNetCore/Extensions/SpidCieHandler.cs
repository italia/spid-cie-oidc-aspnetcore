using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Spid.Cie.OIDC.AspNetCore.Events;
using Spid.Cie.OIDC.AspNetCore.Helpers;
using Spid.Cie.OIDC.AspNetCore.Resources;
using Spid.Cie.OIDC.AspNetCore.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Spid.Cie.OIDC.AspNetCore.Extensions;

class SpidCieHandler : OpenIdConnectHandler
{
    readonly SpidCieEvents _events;
    const string NonceProperty = "N";
    readonly ICryptoService _cryptoService;
    readonly IAggregatorsHandler _aggHandler;
    readonly IRelyingPartiesHandler _rpHandler;
    OpenIdConnectConfiguration? _configuration;
    readonly IIdentityProvidersHandler _idpHandler;
    readonly IHttpContextAccessor _httpContextAccessor;
    readonly IRelyingPartySelector _relyingPartySelector;


    public SpidCieHandler(IOptionsMonitor<OpenIdConnectOptions> options, IHttpContextAccessor httpContextAccessor, ILoggerFactory logger, HtmlEncoder htmlEncoder,
                            UrlEncoder encoder, ISystemClock clock, IIdentityProvidersHandler idpHandler, IRelyingPartiesHandler rpHandler,
                            IRelyingPartySelector relyingPartySelector, ICryptoService cryptoService, IAggregatorsHandler aggHandler, SpidCieEvents events)
        : base(options, logger, htmlEncoder, encoder, clock)
    {
        Events = events;
        _events = events;
        _rpHandler = rpHandler;
        _aggHandler = aggHandler;
        _idpHandler = idpHandler;
        _cryptoService = cryptoService;
        _httpContextAccessor = httpContextAccessor;
        _relyingPartySelector = relyingPartySelector;
    }

    protected new SpidCieEvents Events
    {
        get { return _events; }
        set { base.Events = value; }
    }

    protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        if (string.IsNullOrWhiteSpace(properties.RedirectUri))
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

        var relyingParty = await _relyingPartySelector.GetSelectedRelyingParty();

        if (relyingParty == default)
        {
            var aggregators = await _aggHandler.GetAggregators();
            var uri = new Uri(UriHelper.GetEncodedUrl(_httpContextAccessor.HttpContext.Request))
                        .GetLeftPart(UriPartial.Path)
                        .Replace(SpidCieConst.JsonEntityConfigurationPath, "")
                        .Replace(SpidCieConst.EntityConfigurationPath, "")
                        .Replace(SpidCieConst.CallbackPath, "")
                        .Replace(SpidCieConst.SignedOutCallbackPath, "")
                        .Replace(SpidCieConst.RemoteSignOutPath, "")
                        .EnsureTrailingSlash();

            relyingParty = aggregators.SelectMany(a => a.RelyingParties)
                            .OrderByDescending(r => r.Id.Length)
                            .FirstOrDefault(r => uri.StartsWith(r.Id.EnsureTrailingSlash(), StringComparison.OrdinalIgnoreCase));
        }

        Throw<Exception>.If(relyingParty is null, ErrorLocalization.RelyingPartyNotFound);

        Options.NonceCookie.Path = $"{new Uri(relyingParty!.Id).AbsolutePath.RemoveTrailingSlash()}/{SpidCieConst.CallbackPath.RemoveLeadingSlash()}";

        message.Nonce = Options.ProtocolValidator.GenerateNonce();
        WriteNonceCookie(message.Nonce);

        Options.CorrelationCookie.Path = $"{new Uri(relyingParty!.Id).AbsolutePath.RemoveTrailingSlash()}/{SpidCieConst.CallbackPath.RemoveLeadingSlash()}";

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

        Throw<InvalidOperationException>.If(string.IsNullOrWhiteSpace(message.IssuerAddress),
            "Cannot redirect to the authorization endpoint, the configuration may be missing or invalid.");

        var redirectUri = message.CreateAuthenticationRequestUrl();

        Response.Redirect(redirectUri);
        return;
    }

    private void RestoreOriginalPath()
    {
        if (_httpContextAccessor.HttpContext.Request.Headers.ContainsKey("X-Replaced-Path"))
            _httpContextAccessor.HttpContext.Request.Path = _httpContextAccessor.HttpContext.Request.Headers["X-Replaced-Path"].FirstOrDefault();
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        RestoreOriginalPath();
        return base.HandleAuthenticateAsync();
    }

    protected override Task<HandleRequestResult> HandleRemoteAuthenticateAsync()
    {
        RestoreOriginalPath();
        return base.HandleRemoteAuthenticateAsync();
    }

    protected override Task<bool> HandleSignOutCallbackAsync()
    {
        RestoreOriginalPath();
        return base.HandleSignOutCallbackAsync();
    }

    protected override Task<bool> HandleRemoteSignOutAsync()
    {
        RestoreOriginalPath();
        return base.HandleRemoteSignOutAsync();
    }

    protected override Task HandleForbiddenAsync(AuthenticationProperties properties)
    {
        RestoreOriginalPath();
        return base.HandleForbiddenAsync(properties);
    }

    protected override Task<HandleRequestResult> HandleAccessDeniedErrorAsync(AuthenticationProperties properties)
    {
        RestoreOriginalPath();
        return base.HandleAccessDeniedErrorAsync(properties);
    }

    private void WriteNonceCookie(string nonce)
    {
        Throw<ArgumentNullException>.If(string.IsNullOrWhiteSpace(nonce), nameof(nonce));

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

        var rps = await _rpHandler.GetRelyingParties();
        var rp = rps.FirstOrDefault(r => r.Id.Equals(clientId));
        Throw<InvalidOperationException>.If(rp is null,
            $"No RelyingParty found for the clientId {clientId}");

        Throw<Exception>.If(rp!.OpenIdCoreCertificates is null || rp!.OpenIdCoreCertificates.Count() == 0,
                "No OpenIdCore certificates were found in the currently selected RelyingParty");
        var certificate = rp!.OpenIdCoreCertificates!.FirstOrDefault(occ => occ.KeyUsage == Enums.KeyUsageTypes.Signature)!;

        var revocationEndpoint = idp!.EntityConfiguration.Metadata.OpenIdProvider!.AdditionalData[SpidCieConst.RevocationEndpoint] as string;
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
                Value = _cryptoService.CreateClientAssertion(idp!.EntityConfiguration.Metadata.OpenIdProvider!.AdditionalData["revocation_endpoint"] as string, clientId!, certificate.Certificate!)
            },
            Token = accessToken
        };

        var responseMessage = await Backchannel.RevokeTokenAsync(request);
        if (responseMessage.HttpStatusCode != System.Net.HttpStatusCode.OK
            || responseMessage.IsError)
        {
            Logger.LogWarning($"AccessToken Revocation returned http status {responseMessage.HttpStatusCode} - Error: {responseMessage.Error}");
        }
    }
}