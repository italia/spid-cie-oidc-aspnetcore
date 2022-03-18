using System;
using System.Text.Json.Serialization;

namespace Spid.Cie.OIDC.AspNetCore.Models;

internal class JwtToken
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    [JsonPropertyName("iss")]
    public string Issuer { get; set; }

    [JsonPropertyName("sub")]
    public string Subject { get; set; }

    [JsonPropertyName("jti")]
    public string Jti { get; set; }

    [JsonPropertyName("aud")]
    public string[] Audiences { get; set; }

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
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}
