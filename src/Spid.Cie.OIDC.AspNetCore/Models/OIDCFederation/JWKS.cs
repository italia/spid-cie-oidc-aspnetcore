using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Spid.Cie.OIDC.AspNetCore.Models;

[ExcludeFromCodeCoverage]
sealed class JWKS
{
    [JsonPropertyName("keys")]
    public List<JsonWebKey> Keys { get; set; } = new();
}