﻿using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Spid.Cie.OIDC.AspNetCore.Models;

[ExcludeFromCodeCoverage]
class RPResolveConfiguration : ConfigurationBaseInfo
{
    [JsonPropertyName("metadata")]
    public RPMetadata_SpidCieOIDCConfiguration? Metadata { get; set; }

    [JsonPropertyName("trust_marks")]
    public List<TrustMarkDefinition> TrustMarks { get; set; } = new();

    [JsonPropertyName("trust_chain")]
    public List<string> TrustChain { get; set; } = new();
}