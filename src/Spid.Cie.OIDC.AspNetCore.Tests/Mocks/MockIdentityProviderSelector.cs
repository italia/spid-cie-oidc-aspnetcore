using Spid.Cie.OIDC.AspNetCore.Models;
using Spid.Cie.OIDC.AspNetCore.Services;
using System.Linq;
using System.Threading.Tasks;

namespace Spid.Cie.OIDC.AspNetCore.Tests.Mocks;

internal class MockIdentityProviderSelector : IIdentityProviderSelector
{
    private readonly bool _emptyCollection;

    public MockIdentityProviderSelector(bool emptyCollection)
    {
        _emptyCollection = emptyCollection;
    }

    public async Task<IdentityProvider?> GetSelectedIdentityProvider()
    {
        return (await new MockIdentityProvidersHandler(_emptyCollection).GetIdentityProviders()).FirstOrDefault();
    }
}
