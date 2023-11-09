using Spid.Cie.OIDC.AspNetCore.Services.Defaults;
using System.Threading.Tasks;
using Xunit;

namespace Spid.Cie.OIDC.AspNetCore.Tests;

public class DefaultIdentityProviderRetrieverTests
{

    [Fact]
    public async Task TestGetSpidIdentityProviders()
    {
        bool hasQueryStringValue = true;
        bool emptyIdpCollection = true;
        var _selector = new DefaultIdentityProvidersRetriever();

        var idp = await _selector.GetSpidIdentityProviders();
        Assert.Empty(idp);
    }

    [Fact]
    public async Task TestGetCieIdentityProviders()
    {
        bool hasQueryStringValue = true;
        bool emptyIdpCollection = true;
        var _selector = new DefaultIdentityProvidersRetriever();

        var idp = await _selector.GetCieIdentityProviders();
        Assert.Empty(idp);
    }

}
