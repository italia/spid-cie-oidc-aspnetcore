using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Spid.Cie.OIDC.AspNetCore.Models;

[ExcludeFromCodeCoverage]
internal class TrustMarkPayload
{
    [JsonPropertyName("iss")]
    public string Issuer { get; set; }

    [JsonPropertyName("sub")]
    public string Subject { get; set; }

    [JsonIgnore()]
    public DateTimeOffset IssuedAt { get; set; }

    [JsonPropertyName("iat")]
    public long Iat
    {
        get => IssuedAt.ToUnixTimeSeconds();
        set => IssuedAt = DateTimeOffset.FromUnixTimeSeconds(value);
    }

    [JsonIgnore()]
    public DateTimeOffset ExpiresOn { get; set; }

    [JsonPropertyName("exp")]
    public long Exp
    {
        get => ExpiresOn.ToUnixTimeSeconds();
        set => ExpiresOn = DateTimeOffset.FromUnixTimeSeconds(value);
    }

    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("logo_uri")]
    public string LogoUri { get; set; }

    [JsonPropertyName("ref")]
    public string Ref { get; set; }

    [JsonPropertyName("organization_type")]
    public string OrganizationType { get; set; }

    [JsonPropertyName("organization_name")]
    public string OrganizationName { get; set; }

    [JsonPropertyName("id_code")]
    public string IdCode { get; set; }

    [JsonPropertyName("email")]
    public string Email { get; set; }

    [JsonPropertyName("sa_profile")]
    public string SAProfile { get; set; }

}
