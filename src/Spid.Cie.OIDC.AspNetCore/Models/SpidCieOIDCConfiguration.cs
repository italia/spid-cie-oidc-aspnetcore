using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Spid.Cie.OIDC.AspNetCore.Models;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
[ExcludeFromCodeCoverage]
internal class RequestAuthenticationMethodsSupported
{
    [JsonPropertyName("ar")]
    public List<string> Ar { get; set; }

}

[ExcludeFromCodeCoverage]
internal sealed class SAMetadata_SpidCieOIDCConfiguration
{
    [JsonPropertyName("federation_entity")]
    public SA_SpidCieOIDCFederationEntity FederationEntity { get; set; }
    [JsonPropertyName("trust_mark_issuer")]
    public SA_TrustMarkIssuer TrustMarkIssuer { get; set; }
}

[ExcludeFromCodeCoverage]
internal sealed class SA_TrustMarkIssuer
{
    //[JsonPropertyName("federation_status_endpoint")]
    //public string FederationStatusEndpoint { get; set; }
}

[ExcludeFromCodeCoverage]
internal sealed class RPMetadata_SpidCieOIDCConfiguration
{
    [JsonPropertyName("openid_relying_party")]
    public RP_SpidCieOIDCConfiguration OpenIdRelyingParty { get; set; }
    [JsonPropertyName("federation_entity")]
    public RP_SpidCieOIDCFederationEntity FederationEntity { get; set; }
}

[ExcludeFromCodeCoverage]
internal sealed class RP_SpidCieOIDCConfiguration
{
    [JsonPropertyName("client_registration_types")]
    public List<string> ClientRegistrationTypes { get; } = new() { SpidCieConst.RPClientRegistrationType };

    [JsonPropertyName("application_type")]
    public string ApplicationType { get; } = SpidCieConst.RPApplicationType;

    [JsonPropertyName("client_name")]
    public string ClientName { get; set; }

    [JsonPropertyName("grant_types")]
    public List<string> GrantTypes { get; set; }

    [JsonPropertyName("jwks")]
    public JWKS JWKS { get; set; }

    [JsonPropertyName("redirect_uris")]
    public List<string> RedirectUris { get; set; }

    [JsonPropertyName("response_types")]
    public List<string> ResponseTypes { get; set; }

    [JsonPropertyName("subject_type")]
    public string SubjectType { get; } = SpidCieConst.RPSubjectType;
    [JsonPropertyName("client_id")]
    public string ClientId { get; set; }
}


[ExcludeFromCodeCoverage]
internal sealed class RP_SpidCieOIDCFederationEntity
{
    [JsonPropertyName("organization_name")]
    public string OrganizationName { get; set; }

    [JsonPropertyName("homepage_uri")]
    public string HomepageUri { get; set; }

    [JsonPropertyName("policy_uri")]
    public string PolicyUri { get; set; }

    [JsonPropertyName("logo_uri")]
    public string LogoUri { get; set; }

    [JsonPropertyName("contacts")]
    public List<string> Contacts { get; set; }

    [JsonPropertyName("federation_resolve_endpoint")]
    public string FederationResolveEndpoint { get; set; }
}

[ExcludeFromCodeCoverage]
internal sealed class SA_SpidCieOIDCFederationEntity
{
    [JsonPropertyName("organization_name")]
    public string OrganizationName { get; set; }

    [JsonPropertyName("homepage_uri")]
    public string HomepageUri { get; set; }

    [JsonPropertyName("policy_uri")]
    public string PolicyUri { get; set; }

    [JsonPropertyName("logo_uri")]
    public string LogoUri { get; set; }

    [JsonPropertyName("contacts")]
    public List<string> Contacts { get; set; }

    [JsonPropertyName("federation_resolve_endpoint")]
    public string FederationResolveEndpoint { get; set; }

    [JsonPropertyName("federation_fetch_endpoint")]
    public string FederationFetchEndpoint { get; set; }

    [JsonPropertyName("federation_list_endpoint")]
    public string FederationListEndpoint { get; set; }

    [JsonPropertyName("federation_trust_mark_status_endpoint")]
    public string FederationTrustMarkStatusEndpoint { get; set; }
}

[ExcludeFromCodeCoverage]
internal sealed class IdPMetadata_SpidCieOIDCConfiguration
{
    [JsonPropertyName("openid_provider")]
    public OpenIdConnectConfiguration? OpenIdProvider { get; set; }
}

[ExcludeFromCodeCoverage]
internal sealed class JWKS
{
    [JsonPropertyName("keys")]
    public List<JsonWebKey> Keys { get; set; } = new();
}

[ExcludeFromCodeCoverage]
internal class JsonWebKey
{
    public string kty { get; set; }
    public string use { get; set; }
    public string kid { get; set; }
    //public string x5t { get; set; }
    public string e { get; set; }
    public string n { get; set; }
    //public List<string> x5c { get; set; }
    //public string alg { get; set; }
}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
