using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Spid.Cie.OIDC.AspNetCore.Models;

[ExcludeFromCodeCoverage]
class JsonWebKey
{
    [JsonPropertyName("kty")]
    public string? Kty { get; set; }

    [JsonPropertyName("use")]
    public string? Use { get; set; }

    [JsonPropertyName("kid")]
    public string? Kid { get; set; }

    [JsonPropertyName("e")]
    public string? E { get; set; }

    [JsonPropertyName("n")]
    public string? N { get; set; }

    [JsonPropertyName("alg")]
    public string? Alg { get; set; }

    //[JsonPropertyName("x5t")]
    //public string X5t { get; set; }

    //[JsonPropertyName("x5c")]
    //public List<string> X5c { get; set; } = new();
}