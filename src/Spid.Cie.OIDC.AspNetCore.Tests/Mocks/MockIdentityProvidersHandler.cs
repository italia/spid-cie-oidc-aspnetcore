using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Newtonsoft.Json.Linq;
using Spid.Cie.OIDC.AspNetCore.Models;
using Spid.Cie.OIDC.AspNetCore.Services;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using static Spid.Cie.OIDC.AspNetCore.Tests.Mocks.TestSettings;

namespace Spid.Cie.OIDC.AspNetCore.Tests.Mocks;

internal class MockIdentityProvidersHandler : IIdentityProvidersHandler
{
    private readonly bool _emptyCollection;
    private readonly IEntityConfigurationUtils _ecutils;
    public MockIdentityProvidersHandler(bool emptyCollection = false)
    {
        _emptyCollection = emptyCollection;
        var factory = new Mock<IHttpClientFactory>();
        var logger = new Mock<ILogger<EntityConfigurationUtils>>();
        var loggerP = new Mock<ILogPersister>();
        _ecutils = new EntityConfigurationUtils(factory.Object, new Mocks.MockCryptoService(), loggerP.Object, logger.Object); 
    }

    public async Task<IEnumerable<IdentityProvider>> GetIdentityProviders()
    {
        await Task.CompletedTask;

        var resourceName = "Spid.Cie.OIDC.AspNetCore.Tests.IntegrationTests.jwtOP.json";
        using var stream = typeof(MockBackchannel).Assembly.GetManifestResourceStream(resourceName);
        using var reader = new StreamReader(stream);
        var body = await reader.ReadToEndAsync();
        // var decodedOpJwt = new Mocks.MockCryptoService().DecodeJWT(body);
        //var conf = OpenIdConnectConfiguration.Create(JObject.Parse(decodedOpJwt)["metadata"]["openid_provider"].ToString());
        //this replicate code in trustchainmanager
        (OPEntityConfiguration? opConf, string? decodedOPJwt, string? opJwt) =  _ecutils.ParseEntityConfiguration<OPEntityConfiguration>("https://www.test.it/ec",body);
        //parse openid configuration
        var opJobj = JObject.Parse(decodedOPJwt);
        opConf.Metadata.OpenIdProvider = OpenIdConnectConfiguration.Create(opJobj["metadata"]["openid_provider"].ToString());
        opConf.Metadata.OpenIdProvider.JsonWebKeySet = JsonWebKeySet.Create(opJobj["metadata"]["openid_provider"]["jwks"].ToString());
        return _emptyCollection
        ? Enumerable.Empty<IdentityProvider>()
        : await Task.FromResult(new List<IdentityProvider>() {
                    new SpidIdentityProvider(){
                        Uri = "http://127.0.0.1:8000/oidc/op/",
                        SupportedAcrValues = new() { SpidCieConst.SpidL2, SpidCieConst.SpidL1, SpidCieConst.SpidL3 },
                        EntityConfiguration = opConf
                    },
                    new CieIdentityProvider(){
                        Uri = "http://127.0.0.1:8002/oidc/op/",
                        SupportedAcrValues = new() { SpidCieConst.SpidL2, SpidCieConst.SpidL1, SpidCieConst.SpidL3 },
                        EntityConfiguration = opConf
                    }
            });
    }


}
