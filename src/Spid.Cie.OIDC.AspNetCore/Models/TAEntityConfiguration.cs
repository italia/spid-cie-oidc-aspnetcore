﻿using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Spid.Cie.OIDC.AspNetCore.Models;

[ExcludeFromCodeCoverage]
internal class TAEntityConfiguration : FederationEntityConfiguration
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    [JsonPropertyName("metadata")]
    public TAMetadata_SpidCieOIDCConfiguration Metadata { get; set; }
}



[ExcludeFromCodeCoverage]
internal sealed class TAMetadata_SpidCieOIDCConfiguration
{
    [JsonPropertyName("federation_entity")]
    public TAFederationEntity FederationEntity { get; set; }
}

[ExcludeFromCodeCoverage]
internal sealed class TAFederationEntity
{

    [JsonPropertyName("contacts")]
    public string[] Contacts { get; set; }

    [JsonPropertyName("federation_fetch_endpoint")]
    public string FederationFetchEndpoint { get; set; }

    [JsonPropertyName("homepage_uri")]
    public string HomepageUri { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("federation_list_endpoint")]
    public string FederationListEndpoint { get; set; }

}

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
