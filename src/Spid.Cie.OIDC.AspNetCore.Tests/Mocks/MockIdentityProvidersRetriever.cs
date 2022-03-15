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
                        Name = "MockIdP"
                    }
                });
    }
}
