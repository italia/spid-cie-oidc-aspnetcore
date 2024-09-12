using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Spid.Cie.OIDC.AspNetCore.Models;

[ExcludeFromCodeCoverage]
sealed class SA_SpidCieOIDCFederationEntity
{
    [JsonPropertyName("contacts")]
    public List<string> Contacts { get; set; } = new();

    [JsonPropertyName("federation_fetch_endpoint")]
    public string? FederationFetchEndpoint { get; set; }

    [JsonPropertyName("federation_resolve_endpoint")]
    public string? FederationResolveEndpoint { get; set; }

    [JsonPropertyName("federation_list_endpoint")]
    public string? FederationListEndpoint { get; set; }

    [JsonPropertyName("federation_trust_mark_status_endpoint")]
    public string? FederationTrustMarkStatusEndpoint { get; set; }

    [JsonPropertyName("homepage_uri")]
    public string? HomepageUri { get; set; }

    [JsonPropertyName("organization_name")]
    public string? OrganizationName { get; set; }

    [JsonPropertyName("policy_uri")]
    public string? PolicyUri { get; set; }

    [JsonPropertyName("logo_uri")]
    public string? LogoUri { get; set; }
}