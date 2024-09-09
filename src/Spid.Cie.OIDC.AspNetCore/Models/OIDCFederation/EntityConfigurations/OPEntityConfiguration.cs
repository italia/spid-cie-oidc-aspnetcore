using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Spid.Cie.OIDC.AspNetCore.Models;

[ExcludeFromCodeCoverage]
class OPEntityConfiguration : ExtendedEntityConfiguration
{
    [JsonPropertyName("metadata")]
    public OPMetadata_SpidCieOIDCConfiguration? Metadata { get; set; }
}