using Spid.Cie.OIDC.AspNetCore.Models;
using Spid.Cie.OIDC.AspNetCore.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        return _emptyCollection
            ? Enumerable.Empty<IdentityProvider>()
            : await Task.FromResult(new List<IdentityProvider>() {
                    new SpidIdentityProvider(){
                        Uri = "http://127.0.0.1:8000/oidc/op/",
                        EntityConfiguration = new IdPEntityConfiguration(){
                            Issuer = "http://127.0.0.1:8000/oidc/op/",
                            Metadata = new IdPMetadata_SpidCieOIDCConfiguration(){
                                OpenIdProvider = new Microsoft.IdentityModel.Protocols.OpenIdConnect.OpenIdConnectConfiguration(){
                                    TokenEndpoint = "http://127.0.0.1:8000/oidc/op/token/"
                                }
                            }
                        }
                    },
                    new CieIdentityProvider(){
                        Uri = "http://127.0.0.1:8002/oidc/op/",
                        EntityConfiguration = new IdPEntityConfiguration(){
                            Issuer = "http://127.0.0.1:8002/oidc/op/",
                            Metadata = new IdPMetadata_SpidCieOIDCConfiguration(){
                                OpenIdProvider = new Microsoft.IdentityModel.Protocols.OpenIdConnect.OpenIdConnectConfiguration(){
                                    TokenEndpoint = "http://127.0.0.1:8002/oidc/op/token/"
                                }
                            }
                        }
                    }
                });
    }
}
