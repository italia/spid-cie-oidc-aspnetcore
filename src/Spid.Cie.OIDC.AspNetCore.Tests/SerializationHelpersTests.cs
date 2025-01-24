using Microsoft.Extensions.Configuration;
using Spid.Cie.OIDC.AspNetCore.Helpers;
using Spid.Cie.OIDC.AspNetCore.Models;
using Spid.Cie.OIDC.AspNetCore.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
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

    [Fact]
    public async Task CheckOPResolveSerializationData()
    {
        var service = new CryptoService();
        //recupero identity provider
        var idp = (await new Mocks.MockIdentityProvidersHandler().GetIdentityProviders()).FirstOrDefault();
        //recuperato il codice da ResolveOpenIdFederationMiddleware
        var response = new OPResolveConfiguration()
        {
            ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(SpidCieConst.EntityConfigurationExpirationInMinutes),
            IssuedAt = DateTimeOffset.UtcNow,
            Issuer = "http://127.0.0.1:5000/ta",
            //Metadata = trustChain.OpConf.Metadata,
            Metadata = new OPResolveMetadata_SpidCieOIDCConfiguration()
            {
                OpenIdProvider = idp?.EntityConfiguration?.Metadata?.OpenIdProvider
            },
            Subject = idp.Uri,
            //TrustMarks = trustChain.OpConf.TrustMarks?
            //    .Where(t => JsonSerializer.Deserialize<IdPEntityConfiguration>(cryptoService.DecodeJWT(t.TrustMark)).ExpiresOn >= DateTimeOffset.UtcNow)
            //    .ToList() ?? new List<TrustMarkDefinition>(),
            TrustMarks = idp.EntityConfiguration.TrustMarks?
                        .Where(t => JsonSerializer.Deserialize<OPEntityConfiguration>(service.DecodeJWT(t.TrustMark)).ExpiresOn >= DateTimeOffset.UtcNow)
                        .ToList() ?? new List<TrustMarkDefinition>(),
            TrustChain = new List<string>() //TODO: non corrisponde alla realtà del codice, mi interessa testare la serializzazione di OPResolveMetadata_SpidCieOIDCConfiguration
        };
        Assert.Contains("\"revocation_endpoint\":\"http://127.0.0.1:8000/oidc/op/revocation/\"", JsonSerializer.Serialize(response));
    }

}
