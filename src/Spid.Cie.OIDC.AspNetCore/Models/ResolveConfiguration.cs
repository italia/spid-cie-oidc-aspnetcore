using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Spid.Cie.OIDC.AspNetCore.Models;

[ExcludeFromCodeCoverage]
internal class ResolveConfiguration
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
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

    [JsonPropertyName("metadata")]
    public IdPMetadata_SpidCieOIDCConfiguration Metadata { get; set; }

    [JsonPropertyName("trust_marks")]
    public List<TrustMarkDefinition> TrustMarks { get; set; } = new();

    [JsonPropertyName("trust_chain")]
    public List<string> TrustChain { get; set; } = new();

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}
