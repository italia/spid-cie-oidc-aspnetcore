using Spid.Cie.OIDC.AspNetCore.Services.Defaults;
using System.Threading.Tasks;
using Xunit;

namespace Spid.Cie.OIDC.AspNetCore.Tests;

public class DefaultAggregatorsRetrieverTests
{

    [Fact]
    public async Task TestGetSpidIdentityProviders()
    {
        bool hasQueryStringValue = true;
        bool emptyIdpCollection = true;
        var _selector = new DefaultAggregatorsRetriever();

        var idp = await _selector.GetAggregators();
        Assert.Empty(idp);
    }



}
