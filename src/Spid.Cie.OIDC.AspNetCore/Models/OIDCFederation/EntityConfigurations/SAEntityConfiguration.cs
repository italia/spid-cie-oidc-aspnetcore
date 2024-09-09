using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Spid.Cie.OIDC.AspNetCore.Models;

[ExcludeFromCodeCoverage]
class SAEntityConfiguration : ExtendedEntityConfiguration
{
    [JsonPropertyName("metadata")]
    public SAMetadata_SpidCieOIDCConfiguration? Metadata { get; set; }
}