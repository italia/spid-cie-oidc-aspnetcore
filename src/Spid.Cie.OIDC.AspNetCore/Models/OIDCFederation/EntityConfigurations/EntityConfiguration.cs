using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Spid.Cie.OIDC.AspNetCore.Models;

[ExcludeFromCodeCoverage]
class EntityConfiguration : FederationEntityConfiguration
{
    [JsonPropertyName("authority_hints")]
    public List<string> AuthorityHints { get; set; } = new();
}