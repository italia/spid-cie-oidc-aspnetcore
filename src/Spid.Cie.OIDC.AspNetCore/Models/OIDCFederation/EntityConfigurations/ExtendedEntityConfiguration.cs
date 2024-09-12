using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Spid.Cie.OIDC.AspNetCore.Models;

[ExcludeFromCodeCoverage]
class ExtendedEntityConfiguration : EntityConfiguration
{
    [JsonPropertyName("trust_marks")]
    public List<TrustMarkDefinition> TrustMarks { get; set; } = new();
}