using Microsoft.Extensions.Configuration;
using Spid.Cie.OIDC.AspNetCore.Helpers;
using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Xunit;

namespace Spid.Cie.OIDC.AspNetCore.Tests;

public class SerializationHelpersTests
{
    [Fact]
    public void ToJsonStringTest()
    {
        Assert.Equal($"[{Environment.NewLine}  0,{Environment.NewLine}  1{Environment.NewLine}]", SerializationHelpers.ToJsonString(JsonDocument.Parse("[0,1]")));
    }

    [Fact]
    public void Deserialize()
    {
        var appSettings = @"{""AppSettings"":{
            ""Key1"" : [""Value1"", ""Value1""],
            ""Key2"" : true,
            ""Key3"" : 3
            }}";

        var appSettingsSection = @"{
            ""Key1"" : [""Value1"", ""Value1""],
            ""Key2"" : true,
            ""Key3"" : 3
            }";

        var builder = new ConfigurationBuilder();

        builder.AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(appSettings)));

        var configuration = builder.Build();

        Assert.True(SerializationHelpers.Serialize(configuration.GetSection("AppSettings")).ToJsonString().Equals(JsonNode.Parse(appSettingsSection).ToJsonString()));
    }

}
