using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Spid.Cie.OIDC.AspNetCore.Models;

[ExcludeFromCodeCoverage]
sealed class RP_SpidCieOIDCFederationEntity
{
    [JsonPropertyName("federation_resolve_endpoint")]
    public string? FederationResolveEndpoint { get; set; }

    [JsonPropertyName("organization_name")]
    public string? OrganizationName { get; set; }

    [JsonPropertyName("homepage_uri")]
    public string? HomepageUri { get; set; }

    [JsonPropertyName("policy_uri")]
    public string? PolicyUri { get; set; }

    [JsonPropertyName("logo_uri")]
    public string? LogoUri { get; set; }

    [JsonPropertyName("contacts")]
    public List<string> Contacts { get; set; } = new();
}