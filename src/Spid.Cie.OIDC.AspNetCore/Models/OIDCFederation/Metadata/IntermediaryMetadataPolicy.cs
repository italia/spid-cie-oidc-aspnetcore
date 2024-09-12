using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Spid.Cie.OIDC.AspNetCore.Models
{
    [ExcludeFromCodeCoverage]
    public sealed class IntermediaryMetadataPolicy
    {
        [JsonPropertyName("id_token_signed_response_alg")]
        public IntermediaryTokenSignedResaponseAlgorithm? TokenSignedResaponseAlgorithmId { get; set; }
    }
}