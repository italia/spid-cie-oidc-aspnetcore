using JWT;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Spid.Cie.OIDC.AspNetCore.Helpers;

internal class CustomJsonSerializer : IJsonSerializer
{
    private static readonly JsonSerializerOptions _options = new() { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };

    public T? Deserialize<T>(string json)
        => JsonSerializer.Deserialize<T>(json, _options);

    public string Serialize(object obj)
        => JsonSerializer.Serialize(obj, _options);
}

internal static class SerializationHelpers
{
    public static string ToJsonString(this JsonDocument jdoc)
    {
        using var stream = new MemoryStream();
        Utf8JsonWriter writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true });
        jdoc.WriteTo(writer);
        writer.Flush();
        return Encoding.UTF8.GetString(stream.ToArray());
    }
}