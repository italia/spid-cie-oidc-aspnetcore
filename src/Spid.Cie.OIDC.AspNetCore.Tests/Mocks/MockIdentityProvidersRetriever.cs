using Spid.Cie.OIDC.AspNetCore.Models;
using Spid.Cie.OIDC.AspNetCore.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Spid.Cie.OIDC.AspNetCore.Tests.Mocks;

internal class MockIdentityProvidersRetriever : IIdentityProvidersRetriever
{
    private readonly bool _emptyCollection;

    public MockIdentityProvidersRetriever(bool emptyCollection)
    {
        _emptyCollection = emptyCollection;
    }

    public async Task<IEnumerable<IdentityProvider>> GetIdentityProviders()
    {
        return _emptyCollection
            ? Enumerable.Empty<IdentityProvider>()
            : await Task.FromResult(new List<IdentityProvider>() {
                    new SpidIdentityProvider(){
                        Uri = "http://127.0.0.1/",
                        EntityConfiguration = new IdPEntityConfiguration(){
                            Issuer = "http://127.0.0.1/",
                            Metadata = new IdPMetadata_SpidCieOIDCConfiguration(){
                                OpenIdProvider = new Microsoft.IdentityModel.Protocols.OpenIdConnect.OpenIdConnectConfiguration(){
                                    TokenEndpoint = "http://127.0.0.1/token"
                                }
                            }
                        }
                    }
                });
    }
}
