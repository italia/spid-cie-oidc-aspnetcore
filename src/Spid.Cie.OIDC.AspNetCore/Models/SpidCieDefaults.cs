namespace Spid.Cie.OIDC.AspNetCore.Models;

/// <summary>
/// Default values related to Spid authentication handler
/// </summary>
internal sealed class SpidCieDefaults
{
    /// <summary>
    /// The default authentication type used when registering the SpidHandler.
    /// </summary>
    public const string AuthenticationScheme = "SpidCieOIDC";

    /// <summary>
    /// The default display name used when registering the SpidHandler.
    /// </summary>
    public const string DisplayName = "SpidCieOIDC";

    /// <summary>
    /// Constant used to identify userstate inside AuthenticationProperties that have been serialized in the 'wctx' parameter.
    /// </summary>
    public const string UserstatePropertiesKey = "SpidCieOIDC.Userstate";

    /// <summary>
    /// The cookie name
    /// </summary>
    public const string CookieName = "SpidCieOIDC.Properties";

    public const string ResponseType = "code";

    public const string OpenIdScope = "openid";

    public const string Prompt = "consent";
}
