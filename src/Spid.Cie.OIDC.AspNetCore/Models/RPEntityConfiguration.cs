using System.Text.Json.Serialization;

namespace Spid.Cie.OIDC.AspNetCore.Models;

internal class RPEntityConfiguration : EntityConfiguration
{
    [JsonPropertyName("metadata")]
    public RPMetadata_SpidCieOIDCConfiguration Metadata { get; set; }
}
