using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Spid.Cie.OIDC.AspNetCore.Models;

[ExcludeFromCodeCoverage]
class RPEntityConfiguration : ExtendedEntityConfiguration
{
    [JsonPropertyName("metadata")]
    public RPMetadata_SpidCieOIDCConfiguration? Metadata { get; set; }
}