using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Spid.Cie.OIDC.AspNetCore.Models;

[ExcludeFromCodeCoverage]
class ConfigurationBaseInfo
{
    [JsonPropertyName("iss")]
    public string? Issuer { get; set; }

    [JsonPropertyName("sub")]
    public string? Subject { get; set; }

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
}