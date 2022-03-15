using JWT;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Spid.Cie.OIDC.AspNetCore.Extensions;

internal class JWTSerializer : IJsonSerializer
{
    private static readonly JsonSerializerOptions _options = new JsonSerializerOptions()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };


    public T Deserialize<T>(string json)
    {
        return JsonSerializer.Deserialize<T>(json, _options);
    }

    public string Serialize(object obj)
    {
        return JsonSerializer.Serialize(obj, _options);
    }
}
