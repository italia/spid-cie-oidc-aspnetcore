using Spid.Cie.OIDC.AspNetCore.Enums;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Spid.Cie.OIDC.AspNetCore.Models;

[ExcludeFromCodeCoverage]
internal class GenericError
{
    [JsonIgnore()]
    public ErrorCodes ErrorCode { get; set; }

    [JsonPropertyName("error")]
    public string Error
    {
        get => ErrorCode.ToString();
        set => ErrorCode = Enum.Parse<ErrorCodes>(value);
    }

    [JsonPropertyName("error_description")]
    public string? ErrorDescription { get; set; }
}