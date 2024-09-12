using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Spid.Cie.OIDC.AspNetCore.Models;

[ExcludeFromCodeCoverage]
class TAEntityConfiguration : FederationEntityConfiguration
{
    [JsonPropertyName("metadata")]
    public TAMetadata_SpidCieOIDCConfiguration? Metadata { get; set; }

    //trust_mark_issuers
    //constraints
}