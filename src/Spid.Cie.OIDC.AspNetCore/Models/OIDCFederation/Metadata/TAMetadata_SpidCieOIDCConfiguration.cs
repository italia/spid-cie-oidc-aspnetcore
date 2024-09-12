using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Spid.Cie.OIDC.AspNetCore.Models;

[ExcludeFromCodeCoverage]
sealed class TAMetadata_SpidCieOIDCConfiguration
{
    [JsonPropertyName("federation_entity")]
    public TAFederationEntity? FederationEntity { get; set; }
}