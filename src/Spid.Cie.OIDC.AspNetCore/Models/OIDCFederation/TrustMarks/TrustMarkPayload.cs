using Spid.Cie.OIDC.AspNetCore.Models.OIDCFederation.TrustMarks;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Spid.Cie.OIDC.AspNetCore.Models;

[ExcludeFromCodeCoverage]
class TrustMarkPayload : ConfigurationBaseInfo
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("logo_uri")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? LogoUri { get; set; }

    [JsonPropertyName("ref")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Ref { get; set; }

    [JsonPropertyName("organization_type")]
    public string? OrganizationType { get; set; }

    [JsonPropertyName("organization_name")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? OrganizationName { get; set; }

    [JsonPropertyName("id_code")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public TrustMarkGovernmentIndex? IdCode { get; set; }

    [JsonPropertyName("email")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Email { get; set; }

    [JsonPropertyName("sa_profile")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? SAProfile { get; set; }
}