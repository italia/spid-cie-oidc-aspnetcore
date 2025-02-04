using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Spid.Cie.OIDC.AspNetCore.Models;

[ExcludeFromCodeCoverage]
sealed class OPResolveMetadata_SpidCieOIDCConfiguration
{
    [JsonPropertyName("openid_provider")]
    public OpenIdConnectConfiguration? OpenIdProvider { get; set; }
}