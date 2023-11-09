using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Spid.Cie.OIDC.AspNetCore.Models;

[ExcludeFromCodeCoverage]
internal class GenericError
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    [JsonIgnore()]
    public ErrorCode ErrorCode { get; set; }

    [JsonPropertyName("error")]
    public string Error
    {
        get => ErrorCode.ToString();
        set => ErrorCode = Enum.Parse<ErrorCode>(value);
    }


    [JsonPropertyName("error_description")]
    public string ErrorDescription { get; set; }

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}

internal enum ErrorCode
{
    invalid_request,
    invalid_client,
    not_found,
    server_error,
    temporarily_unavailable,
    unsupported_parameter
}