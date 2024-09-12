using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Spid.Cie.OIDC.AspNetCore.Models;

[ExcludeFromCodeCoverage]
class SAJWKSValue
{
    [JsonPropertyName("value")]
    public JWKS? JWKS { get; set; }
}