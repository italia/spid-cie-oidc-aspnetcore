using System.Text.Json.Serialization;

namespace Spid.Cie.OIDC.AspNetCore.Models;

internal class IdPEntityConfiguration : EntityConfiguration
{
    [JsonPropertyName("metadata")]
    public IdPMetadata_SpidCieOIDCConfiguration Metadata { get; set; }
}