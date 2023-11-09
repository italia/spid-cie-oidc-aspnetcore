using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Spid.Cie.OIDC.AspNetCore.Models;

[ExcludeFromCodeCoverage]
public sealed class TrustMarkDefinition
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    [JsonPropertyName("id")]
    public string Id { get; set; }
    [JsonPropertyName("iss")]
    public string Issuer { get; set; }

    [JsonPropertyName("trust_mark")]
    public string TrustMark { get; set; }

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}
