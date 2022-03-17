using System.Text.Json;
using System.Text.Json.Serialization;

namespace Spid.Cie.OIDC.AspNetCore.Models;

internal class EntityStatement : FederationEntityConfiguration
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    [JsonPropertyName("metadata_policy")]
    public JsonDocument MetadataPolicy { get; set; }

    [JsonPropertyName("trust_marks")]
    public TrustMarkDefinition[] TrustMarks { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}
