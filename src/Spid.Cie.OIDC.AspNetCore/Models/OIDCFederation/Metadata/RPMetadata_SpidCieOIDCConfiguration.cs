using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Spid.Cie.OIDC.AspNetCore.Models;

[ExcludeFromCodeCoverage]
sealed class RPMetadata_SpidCieOIDCConfiguration
{
    [JsonPropertyName("openid_relying_party")]
    public RP_SpidCieOIDCConfiguration? OpenIdRelyingParty { get; set; }

    [JsonPropertyName("federation_entity")]
    public RP_SpidCieOIDCFederationEntity? FederationEntity { get; set; }
}