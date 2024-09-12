using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Spid.Cie.OIDC.AspNetCore.Models;

[ExcludeFromCodeCoverage]
sealed class TAFederationEntity
{
    [JsonPropertyName("name")]
    public string? Name { get; set; } //"organization_name": "example TA" ?

    [JsonPropertyName("contacts")]
    public string[] Contacts { get; set; } = Array.Empty<string>();

    //"policy_uri": "https://registry.agid.gov.it/policy",

    [JsonPropertyName("homepage_uri")]
    public string? HomepageUri { get; set; }

    //"logo_uri":"https://registry.agid.gov.it/static/svg/logo.svg",

    [JsonPropertyName("federation_fetch_endpoint")]
    public string? FederationFetchEndpoint { get; set; }

    //"federation_resolve_endpoint": "https://registry.agid.gov.it/resolve/",

    [JsonPropertyName("federation_list_endpoint")]
    public string? FederationListEndpoint { get; set; }

    //"federation_trust_mark_status_endpoint": "https://registry.agid.gov.it/trust_mark_status/"
}