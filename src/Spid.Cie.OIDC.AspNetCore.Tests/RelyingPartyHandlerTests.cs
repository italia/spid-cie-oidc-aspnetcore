using Spid.Cie.OIDC.AspNetCore.Services;
using Spid.Cie.OIDC.AspNetCore.Services.Defaults;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Spid.Cie.OIDC.AspNetCore.Tests;

public class RelyingPartyHandlerTests
{
    [Fact]
    public async Task TestGetRelyingParties()
    {
        var retriever = new RelyingPartiesHandler(new Mocks.MockOptionsMonitorSpidCieOptions(), new DefaultRelyingPartiesRetriever());
        var rp = await retriever.GetRelyingParties();
        Assert.NotNull(rp);
        Assert.True(rp.Count() != 0);
    }

    [Fact]
    public async Task TestGetRelyingPartiesEmpty()
    {
        var retriever = new RelyingPartiesHandler(new Mocks.MockOptionsMonitorSpidCieOptions(true), new DefaultRelyingPartiesRetriever());
        var rp = await retriever.GetRelyingParties();
        Assert.NotNull(rp);
        Assert.True(rp.Count() == 0);
    }
}
