using System.Text.Json.Serialization;

namespace Spid.Cie.OIDC.AspNetCore.Models;

internal class IdPEntityConfiguration : EntityConfiguration
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    [JsonPropertyName("metadata")]
    public IdPMetadata_SpidCieOIDCConfiguration Metadata { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}