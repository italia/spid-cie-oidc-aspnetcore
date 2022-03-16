using JWT;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Spid.Cie.OIDC.AspNetCore.Helpers;

internal class SerializationHelpers : IJsonSerializer
{
    private static readonly JsonSerializerOptions _options = new() { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };

    public T? Deserialize<T>(string json)
        => JsonSerializer.Deserialize<T>(json, _options);

    public string Serialize(object obj)
        => JsonSerializer.Serialize(obj, _options);
}
