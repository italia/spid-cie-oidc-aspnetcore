using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Spid.Cie.OIDC.AspNetCore.Models;

[ExcludeFromCodeCoverage]
sealed class SAMetadata_SpidCieOIDCConfiguration
{
    [JsonPropertyName("federation_entity")]
    public SA_SpidCieOIDCFederationEntity? FederationEntity { get; set; }

    //[JsonPropertyName("trust_mark_issuer")]
    //public SA_TrustMarkIssuer? TrustMarkIssuer { get; set; }
}