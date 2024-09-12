using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Spid.Cie.OIDC.AspNetCore.Models
{
    [ExcludeFromCodeCoverage]
    public sealed class IntermediaryTokenSignedResaponseAlgorithm
    {
        [JsonPropertyName("essential")]
        public bool Essential { get; set; } = true;

        [JsonPropertyName("one_of")]
        public List<string> ValidAlgorithms { get; set; } = new() { "RS256", "RS512", "ES256", "ES512", "PS256", "PS512" };
    }
}