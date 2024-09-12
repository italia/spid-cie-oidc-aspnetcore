using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Diagnostics.CodeAnalysis;

namespace Spid.Cie.OIDC.AspNetCore.Events;

[ExcludeFromCodeCoverage]
class PostStateCreatedContext : PropertiesContext<OpenIdConnectOptions>
{
    public PostStateCreatedContext(
        HttpContext context,
        AuthenticationScheme scheme,
        OpenIdConnectOptions options,
        AuthenticationProperties properties)
        : base(context, scheme, options, properties) { }

    /// <summary>
    /// Gets or sets the <see cref="OpenIdConnectMessage"/>.
    /// </summary>
    public OpenIdConnectMessage ProtocolMessage { get; set; } = default!;

    /// <summary>
    /// If true, will skip any default logic for this redirect.
    /// </summary>
    public bool Handled { get; private set; }

    /// <summary>
    /// Skips any default logic for this redirect.
    /// </summary>
    public void HandleResponse() => Handled = true;
}
