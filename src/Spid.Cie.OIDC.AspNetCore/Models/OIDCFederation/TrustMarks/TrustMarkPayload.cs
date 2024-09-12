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
    public string? LogoUri { get; set; }

    [JsonPropertyName("ref")]
    public string? Ref { get; set; }

    [JsonPropertyName("organization_type")]
    public string? OrganizationType { get; set; }

    [JsonPropertyName("organization_name")]
    public string? OrganizationName { get; set; }

    [JsonPropertyName("id_code")]
    public TrustMarkGovernmentIndex? IdCode { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("sa_profile")]
    public string? SAProfile { get; set; }
}