using JWT;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Spid.Cie.OIDC.AspNetCore.Helpers;

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

    public static JsonNode? Serialize(IConfigurationSection config)
    {
        JsonObject obj = new();

        if (config is not null)
        {
            foreach (var child in config.GetChildren())
            {
                if (child.Path.EndsWith(":0"))
                {
                    var arr = new JsonArray();

                    foreach (var arrayChild in config.GetChildren())
                    {
                        arr.Add(Serialize(arrayChild));
                    }

                    return arr;
                }
                else
                {
                    obj.Add(child.Key, Serialize(child));
                }
            }

            if (obj.Count() == 0 && config is IConfigurationSection section)
            {
                if (bool.TryParse(section.Value, out bool boolean))
                {
                    return JsonValue.Create(boolean);
                }
                else if (decimal.TryParse(section.Value, out decimal real))
                {
                    return JsonValue.Create(real);
                }
                else if (long.TryParse(section.Value, out long integer))
                {
                    return JsonValue.Create(integer);
                }

                return JsonValue.Create(section.Value);
            }
        }
        return obj;
    }
}

internal class STJSerializer : IJsonSerializer
{
    public object Deserialize(Type type, string json)
    {
        return JsonSerializer.Deserialize(json, type);
    }

    public string Serialize(object obj)
    {
        return JsonSerializer.Serialize(obj);
    }
}