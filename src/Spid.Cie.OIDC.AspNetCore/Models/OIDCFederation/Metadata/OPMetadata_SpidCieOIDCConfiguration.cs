using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Spid.Cie.OIDC.AspNetCore.Models;

[ExcludeFromCodeCoverage]
sealed class OPMetadata_SpidCieOIDCConfiguration
{
    [JsonPropertyName("openid_provider")]
    public OPMetadata_OpenIdConnectConfiguration? OpenIdProvider { get; set; }
}