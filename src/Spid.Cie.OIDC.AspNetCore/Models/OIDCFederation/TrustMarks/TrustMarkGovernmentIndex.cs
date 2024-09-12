using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;

namespace Spid.Cie.OIDC.AspNetCore.Models.OIDCFederation.TrustMarks;

[ExcludeFromCodeCoverage]
class TrustMarkGovernmentIndex
{
    [JsonProperty("ipa_code")]
    public string? Code { get; set; }
}