using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Spid.Cie.OIDC.AspNetCore.Models;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
[ExcludeFromCodeCoverage]
internal class RequestAuthenticationMethodsSupported
{
    [JsonPropertyName("ar")]
    public string[] Ar { get; set; }

}

[ExcludeFromCodeCoverage]
internal sealed class RPMetadata_SpidCieOIDCConfiguration
{
    [JsonPropertyName("openid_relying_party")]
    public RP_SpidCieOIDCConfiguration OpenIdRelyingParty { get; set; }
}

[ExcludeFromCodeCoverage]
internal sealed class RP_SpidCieOIDCConfiguration
{
    [JsonPropertyName("client_registration_types")]
    public string[] ClientRegistrationTypes { get; set; } = new[] { SpidCieConst.RPClientRegistrationType };

    [JsonPropertyName("application_type")]
    public string ApplicationType { get; set; } = SpidCieConst.RPApplicationType;

    [JsonPropertyName("client_name")]
    public string ClientName { get; set; }

    [JsonPropertyName("contacts")]
    public string[] Contacts { get; set; }

    [JsonPropertyName("grant_types")]
    public string[] GrantTypes { get; set; }

    [JsonPropertyName("jwks")]
    public JWKS JWKS { get; set; }

    [JsonPropertyName("redirect_uris")]
    public string[] RedirectUris { get; set; }

    [JsonPropertyName("response_types")]
    public string[] ResponseTypes { get; set; }

    [JsonPropertyName("subject_type")]
    public string SubjectType { get; set; } = SpidCieConst.RPSubjectType;

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
    public JsonWebKey[] Keys { get; set; }
}

[ExcludeFromCodeCoverage]
internal class JsonWebKey
{
    public string kty { get; set; }
    public string use { get; set; }
    public string kid { get; set; }
    public string x5t { get; set; }
    public string e { get; set; }
    public string n { get; set; }
    public string[] x5c { get; set; }
    public string alg { get; set; }

    public string x { get; set; }
    public string y { get; set; }
    public string crv { get; set; }
    public string d { get; set; }
    public string p { get; set; }
    public string q { get; set; }
    public string dp { get; set; }
    public string dq { get; set; }
    public string qi { get; set; }
}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
