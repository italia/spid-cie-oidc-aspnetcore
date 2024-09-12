using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Spid.Cie.OIDC.AspNetCore.Models;

[ExcludeFromCodeCoverage]
class SA_SpidCieOIDCConfiguration
{
    [JsonPropertyName("jwks")]
    public SAJWKSValue? Value { get; set; }
}