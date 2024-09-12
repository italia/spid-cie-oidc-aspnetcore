using System;
using System.Diagnostics.CodeAnalysis;

namespace Spid.Cie.OIDC.AspNetCore;

/// <summary>
/// Default values related to Spid authentication handler
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class SpidCieConst
{
    public const string Kid = "kid";
    public const string Typ = "typ";
    public const string Iss = "iss";
    public const string Sub = "sub";
    public const string Iat = "iat";
    public const string Exp = "exp";
    public const string Aud = "aud";
    public const string Jti = "jti";
    public const string Nonce = "nonce";
    public const string Scope = "scope";
    public const string State = "state";
    public const string Token = "token";
    public const string Claims = "claims";
    public const string DefaultAcr = SpidL2;
    public const string ResponseType = "code";
    public const string OpenIdScope = "openid";
    public const string ClientId = "client_id";
    public const string AcrValues = "acr_values";
    public const string Prompt = "consent login";
    public const string ListEndpointPath = "list";
    public const string RPApplicationType = "web";
    public const string RPSubjectType = "pairwise";
    public const string PromptParameter = "prompt";
    public const string RPSelectorKey = "clientId";
    public const string IdPSelectorKey = "provider";
    public const string DisplayName = "SpidCieOIDC";// The default display name used when registering the SpidHandler.
    public const string FetchEndpointPath = "fetch";
    public const string RedirectUri = "redirect_uri";
    public const string RequestParameter = "request";
    public const string DummyUrl = "https://dummy.org";
    public const string RefreshToken = "refresh_token";
    public const string OfflineScope = "offline_access";
    public const string ResolveEndpointPath = "resolve";
    public const string JWKGeneratorPath = "generatejwk";
    public const string CodeChallenge = "code_challenge";
    public const string CallbackPath = "/signin-oidc-spidcie";
    public const string TypValue = "entity-statement+jwt";
    public const string ClientAssertion = "client_assertion";
    public const string JsonContentType = "application/json";
    public const string AuthenticationScheme = "SpidCieOIDC";// The default authentication type used when registering the SpidHandler.
    public const string CookieName = "SpidCieOIDC.Properties"; // The cookie name
    public const string RPClientRegistrationType = "automatic";
    public const string RemoteSignOutPath = "/signout-spidcie";
    public const string ResponseTypeParameter = "response_type";
    public const string AuthorizationCode = "authorization_code";
    public const string OPListPath = "list/?type=openid_provider";
    public const string RevocationEndpoint = "revocation_endpoint";
    public const int EntityConfigurationExpirationInMinutes = 2880;
    public const string BackchannelClientName = "SpidCieBackchannel";
    public const string JWKGeneratorContentType = "application/json";
    public const string CodeChallengeMethod = "code_challenge_method";
    public const string ClientAssertionType = "client_assertion_type";
    public const string SpidLevelBaseURI = "https://www.spid.gov.it/";
    public const string SpidL1 = $"{SpidLevelBaseURI}{nameof(SpidL1)}";
    public const string SpidL2 = $"{SpidLevelBaseURI}{nameof(SpidL2)}";
    public const string SpidL3 = $"{SpidLevelBaseURI}{nameof(SpidL3)}";
    public const string UserstatePropertiesKey = "SpidCieOIDC.Userstate";// Constant used to identify userstate inside AuthenticationProperties that have been serialized in the 'wctx' parameter.
    public const string TrustMarkStatusEndpointPath = "trust_mark_status";
    public const string SignedOutCallbackPath = "/signout-callback-spidcie";
    public const string ResolveContentType = "application/resolve-response+jwt";
    public const string EntityConfigurationPath = ".well-known/openid-federation";
    public static TimeSpan TrustChainExpirationGracePeriod = TimeSpan.FromHours(24);
    public const string JsonEntityConfigurationPath = ".well-known/openid-federation/json";
    public const string EntityConfigurationContentType = "application/entity-statement+jwt";
	public const string ClientAssertionTypeValue = "urn:ietf:params:oauth:client-assertion-type:jwt-bearer";
}