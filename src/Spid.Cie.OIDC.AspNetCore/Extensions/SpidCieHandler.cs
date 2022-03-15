using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.Net.Http.Headers;
using Spid.Cie.OIDC.AspNetCore.Events;
using Spid.Cie.OIDC.AspNetCore.Logging;
using Spid.Cie.OIDC.AspNetCore.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Spid.Cie.OIDC.AspNetCore.Extensions;

internal class SpidCieHandler : OpenIdConnectHandler
{
    private OpenIdConnectConfiguration? _configuration;
    private const string NonceProperty = "N";
    private const string HeaderValueEpocDate = "Thu, 01 Jan 1970 00:00:00 GMT";
    private readonly ILogPersister _logPersister;
    private readonly SpidCieEvents _events;
    private readonly IRelyingPartySelector _rpSelector;
    private readonly IIdentityProviderSelector _idpSelector;

    public SpidCieHandler(IOptionsMonitor<OpenIdConnectOptions> options,
            ILoggerFactory logger,
            HtmlEncoder htmlEncoder,
            UrlEncoder encoder,
            ISystemClock clock,
            ILogPersister logPersister,
            SpidCieEvents events)
        : base(options, logger, htmlEncoder, encoder, clock)
    {
        _logPersister = logPersister;
        _events = events;
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

        if (Options.AuthenticationMethod == OpenIdConnectRedirectBehavior.RedirectGet)
        {
            var redirectUri = message.CreateAuthenticationRequestUrl();

            Response.Redirect(redirectUri);
            return;
        }
        else if (Options.AuthenticationMethod == OpenIdConnectRedirectBehavior.FormPost)
        {
            var content = message.BuildFormPost();
            var buffer = Encoding.UTF8.GetBytes(content);

            Response.ContentLength = buffer.Length;
            Response.ContentType = "text/html;charset=UTF-8";

            Response.Headers[HeaderNames.CacheControl] = "no-cache, no-store";
            Response.Headers[HeaderNames.Pragma] = "no-cache";
            Response.Headers[HeaderNames.Expires] = HeaderValueEpocDate;

            await Response.Body.WriteAsync(buffer);
            return;
        }

        throw new NotImplementedException($"An unsupported authentication method has been configured: {Options.AuthenticationMethod}");
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
}
