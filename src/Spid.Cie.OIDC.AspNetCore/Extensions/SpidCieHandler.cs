using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Spid.Cie.OIDC.AspNetCore.Events;
using Spid.Cie.OIDC.AspNetCore.Helpers;
using Spid.Cie.OIDC.AspNetCore.Logging;
using Spid.Cie.OIDC.AspNetCore.Models;
using Spid.Cie.OIDC.AspNetCore.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
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
    private readonly IIdentityProvidersRetriever _idpRetriever;
    private readonly IRelyingPartiesRetriever _rpRetriever;

    public SpidCieHandler(IOptionsMonitor<OpenIdConnectOptions> options,
            ILoggerFactory logger,
            HtmlEncoder htmlEncoder,
            UrlEncoder encoder,
            ISystemClock clock,
            ILogPersister logPersister,
            IIdentityProvidersRetriever idpRetriever,
            IRelyingPartiesRetriever rpRetriever,
            SpidCieEvents events)
        : base(options, logger, htmlEncoder, encoder, clock)
    {
        _logPersister = logPersister;
        _events = events;
        _idpRetriever = idpRetriever;
        _rpRetriever = rpRetriever;
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

        if (Options.UsePkce && Options.ResponseType == OpenIdConnectResponseType.Code)
        {
            var bytes = new byte[32];
            RandomNumberGenerator.Fill(bytes);
            var codeVerifier = Microsoft.AspNetCore.Authentication.Base64UrlTextEncoder.Encode(bytes);

            properties.Items.Add(OAuthConstants.CodeVerifierKey, codeVerifier);

            using SHA256 sha = SHA256.Create();
            var challengeBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier));
            var codeChallenge = WebEncoders.Base64UrlEncode(challengeBytes);

            message.Parameters.Add(OAuthConstants.CodeChallengeKey, codeChallenge);
            message.Parameters.Add(OAuthConstants.CodeChallengeMethodKey, OAuthConstants.CodeChallengeMethodS256);
        }

        var maxAge = properties.GetParameter<TimeSpan?>(OpenIdConnectParameterNames.MaxAge) ?? Options.MaxAge;
        if (maxAge.HasValue)
        {
            message.MaxAge = Convert.ToInt64(Math.Floor((maxAge.Value).TotalSeconds))
                .ToString(CultureInfo.InvariantCulture);
        }

        if (!string.Equals(Options.ResponseType, OpenIdConnectResponseType.Code, StringComparison.Ordinal) ||
            !string.Equals(Options.ResponseMode, OpenIdConnectResponseMode.Query, StringComparison.Ordinal))
        {
            message.ResponseMode = Options.ResponseMode;
        }

        if (Options.ProtocolValidator.RequireNonce)
        {
            message.Nonce = Options.ProtocolValidator.GenerateNonce();
            WriteNonceCookie(message.Nonce);
        }

        GenerateCorrelationId(properties);

        var redirectContext = new RedirectContext(Context, Scheme, Options, properties)
        {
            ProtocolMessage = message
        };

        await Events.RedirectToIdentityProvider(redirectContext);
        if (redirectContext.Handled)
        {
            return;
        }

        message = redirectContext.ProtocolMessage;

        if (!string.IsNullOrEmpty(message.State))
        {
            properties.Items[OpenIdConnectDefaults.UserstatePropertiesKey] = message.State;
        }

        properties.Items.Add(OpenIdConnectDefaults.RedirectUriForCodePropertiesKey, message.RedirectUri);

        message.State = Options.StateDataFormat.Protect(properties);

        await Events.PostStateCreated(new PostStateCreatedContext(Context, Scheme, Options, properties)
        {
            ProtocolMessage = message
        });

        if (string.IsNullOrEmpty(message.IssuerAddress))
        {
            throw new InvalidOperationException("Cannot redirect to the authorization endpoint, the configuration may be missing or invalid.");
        }

        var redirectUri = message.CreateAuthenticationRequestUrl();

        Response.Redirect(redirectUri);
        return;
    }

    private void WriteNonceCookie(string nonce)
    {
        if (string.IsNullOrEmpty(nonce))
        {
            throw new ArgumentNullException(nameof(nonce));
        }

        var cookieOptions = Options.NonceCookie.Build(Context, Clock.UtcNow);

        Response.Cookies.Append(
            Options.NonceCookie.Name + Options.StringDataFormat.Protect(nonce),
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

        var refreshToken = await Context.GetTokenAsync(Options.SignOutScheme, OpenIdConnectParameterNames.RefreshToken);
        if (!string.IsNullOrWhiteSpace(refreshToken))
        {
            await RevokeToken(refreshToken);
        }

        await Context.SignOutAsync(Options.SignOutScheme);
        Response.Redirect(properties?.RedirectUri ?? Options.SignedOutRedirectUri);
    }

    private async Task RevokeToken(string accessToken)
    {
        var issuer = Context.User.FindFirst(SpidCieConst.Iss)?.Value;
        if (!string.IsNullOrWhiteSpace(issuer))
        {
            var idps = await _idpRetriever.GetIdentityProviders();
            var idp = idps.FirstOrDefault(i => i.EntityConfiguration.Issuer.Equals(issuer));
            if (idp != null)
            {
                var revocationEndpoint = idp.EntityConfiguration.Metadata.OpenIdProvider.AdditionalData[SpidCieConst.RevocationEndpoint] as string;
                if (!string.IsNullOrWhiteSpace(revocationEndpoint))
                {
                    var clientId = Context.User.FindFirst(SpidCieConst.Aud)?.Value;
                    if (!string.IsNullOrWhiteSpace(clientId))
                    {
                        var rps = await _rpRetriever.GetRelyingParties();
                        var rp = rps.FirstOrDefault(r => r.ClientId.Equals(clientId));
                        if (rp != null)
                        {
                            var keySet = rp.OpenIdCoreJWKs;
                            var key = keySet?.Keys?.FirstOrDefault();
                            if (key is not null)
                            {
                                (RSA publicKey, RSA privateKey) = key.GetRSAKeys();
                                var requestMessage = new HttpRequestMessage(HttpMethod.Post, revocationEndpoint)
                                {
                                    Version = Backchannel.DefaultRequestVersion,
                                    Content = new FormUrlEncodedContent((IEnumerable<KeyValuePair<string?, string?>>)new Dictionary<string, string>()
                                    {
                                        { SpidCieConst.ClientId, clientId },
                                        { SpidCieConst.ClientAssertionType, SpidCieConst.ClientAssertionTypeValue },
                                        { SpidCieConst.Token, accessToken },
                                        { SpidCieConst.ClientAssertion, CryptoHelpers.CreateJWT(publicKey,
                                            privateKey,
                                            new Dictionary<string, object>() {
                                                { SpidCieConst.Kid, key.Kid },
                                                { SpidCieConst.Typ, SpidCieConst.TypValue }
                                            },
                                            new Dictionary<string, object>() {
                                                { SpidCieConst.Iss, clientId },
                                                { SpidCieConst.Sub, clientId },
                                                { SpidCieConst.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
                                                { SpidCieConst.Exp, DateTimeOffset.UtcNow.AddMinutes(SpidCieConst.EntityConfigurationExpirationInMinutes).ToUnixTimeSeconds() },
                                                { SpidCieConst.Aud, new string[] { revocationEndpoint } },
                                                { SpidCieConst.Jti, Guid.NewGuid().ToString() }
                                            })
                                        }
                                    })
                                };
                                var responseMessage = await Backchannel.SendAsync(requestMessage, Context.RequestAborted);
                                responseMessage.EnsureSuccessStatusCode();
                                var revokeResponse = await responseMessage.Content.ReadAsStringAsync();
                            }
                        }
                    }
                }
            }
        }
    }
}
