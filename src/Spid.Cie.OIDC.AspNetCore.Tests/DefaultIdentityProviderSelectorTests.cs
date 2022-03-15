using Spid.Cie.OIDC.AspNetCore.Services;
using Spid.Cie.OIDC.AspNetCore.Tests.Mocks;
using System.Threading.Tasks;
using Xunit;

namespace Spid.Cie.OIDC.AspNetCore.Tests;

public class DefaultIdentityProviderSelectorTests
{

    [Fact]
    public async Task TestGetSelectedIdentityProvider1()
    {
        bool hasQueryStringValue = true;
        bool emptyIdpCollection = true;
        var _selector = new DefaultIdentityProviderSelector(new MockHttpContextAccessor(hasQueryStringValue), new MockIdentityProvidersRetriever(emptyIdpCollection));

        var idp = await _selector.GetSelectedIdentityProvider();
        Assert.Null(idp);
    }

    [Fact]
    public async Task TestGetSelectedIdentityProvider2()
    {
        bool hasQueryStringValue = true;
        bool emptyIdpCollection = false;
        var _selector = new DefaultIdentityProviderSelector(new MockHttpContextAccessor(hasQueryStringValue), new MockIdentityProvidersRetriever(emptyIdpCollection));

        var idp = await _selector.GetSelectedIdentityProvider();
        Assert.NotNull(idp);
    }

    [Fact]
    public async Task TestGetSelectedIdentityProvider3()
    {
        bool hasQueryStringValue = false;
        bool emptyIdpCollection = true;
        var _selector = new DefaultIdentityProviderSelector(new MockHttpContextAccessor(hasQueryStringValue), new MockIdentityProvidersRetriever(emptyIdpCollection));

        var idp = await _selector.GetSelectedIdentityProvider();
        Assert.Null(idp);
    }

    [Fact]
    public async Task TestGetSelectedIdentityProvider4()
    {
        bool hasQueryStringValue = false;
        bool emptyIdpCollection = false;
        var _selector = new DefaultIdentityProviderSelector(new MockHttpContextAccessor(hasQueryStringValue), new MockIdentityProvidersRetriever(emptyIdpCollection));

        var idp = await _selector.GetSelectedIdentityProvider();
        Assert.Null(idp);
    }
}
