using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Spid.Cie.OIDC.AspNetCore.Models;

[ExcludeFromCodeCoverage]
class EntityStatement : ExtendedEntityConfiguration
{
    [JsonPropertyName("metadata_policy")]
    public JsonDocument? MetadataPolicy { get; set; }

    [JsonPropertyName("openid_relying_party")]
    public SA_SpidCieOIDCConfiguration? OpenIdRelyingParty { get; set; }
}