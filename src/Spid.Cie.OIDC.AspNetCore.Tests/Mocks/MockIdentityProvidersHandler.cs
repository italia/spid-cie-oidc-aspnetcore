using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using Spid.Cie.OIDC.AspNetCore.Models;
using Spid.Cie.OIDC.AspNetCore.Services;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static Spid.Cie.OIDC.AspNetCore.Tests.Mocks.TestSettings;

namespace Spid.Cie.OIDC.AspNetCore.Tests.Mocks;

internal class MockIdentityProvidersHandler : IIdentityProvidersHandler
{
    private readonly bool _emptyCollection;

    public MockIdentityProvidersHandler(bool emptyCollection = false)
    {
        _emptyCollection = emptyCollection;
    }

    public async Task<IEnumerable<IdentityProvider>> GetIdentityProviders()
    {
        await Task.CompletedTask;

        var resourceName = "Spid.Cie.OIDC.AspNetCore.Tests.IntegrationTests.jwtOP.json";
        using var stream = typeof(MockBackchannel).Assembly.GetManifestResourceStream(resourceName);
        using var reader = new StreamReader(stream);
        var body = await reader.ReadToEndAsync();
        var decodedOpJwt = new Mocks.MockCryptoService().DecodeJWT(body);
        var conf = OpenIdConnectConfiguration.Create(JObject.Parse(decodedOpJwt)["metadata"]["openid_provider"].ToString());
        conf.JsonWebKeySet = JsonWebKeySet.Create(JObject.Parse(decodedOpJwt)["metadata"]["openid_provider"]["jwks"].ToString());
        return _emptyCollection
        ? Enumerable.Empty<IdentityProvider>()
        : await Task.FromResult(new List<IdentityProvider>() {
                    new SpidIdentityProvider(){
                        Uri = "http://127.0.0.1:8000/oidc/op/",
                        SupportedAcrValues = new() { SpidCieConst.SpidL2, SpidCieConst.SpidL1, SpidCieConst.SpidL3 },
                        EntityConfiguration = new OPEntityConfiguration(){
                            Issuer = "http://127.0.0.1:8000/oidc/op/",
                            Metadata = new OPMetadata_SpidCieOIDCConfiguration(){
                                OpenIdProvider = conf
                            }
                        }
                    },
                    new CieIdentityProvider(){
                        Uri = "http://127.0.0.1:8002/oidc/op/",
                        SupportedAcrValues = new() { SpidCieConst.SpidL2, SpidCieConst.SpidL1, SpidCieConst.SpidL3 },
                        EntityConfiguration = new OPEntityConfiguration(){
                            Issuer = "http://127.0.0.1:8002/oidc/op/",
                            Metadata = new OPMetadata_SpidCieOIDCConfiguration(){
                                OpenIdProvider = conf
                            }
                        }
                    }
            });
    }


}
