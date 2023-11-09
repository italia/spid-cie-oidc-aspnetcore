using Spid.Cie.OIDC.AspNetCore.Services;
using Spid.Cie.OIDC.AspNetCore.Services.Defaults;
using Spid.Cie.OIDC.AspNetCore.Tests.Mocks;
using System.Threading.Tasks;
using Xunit;

namespace Spid.Cie.OIDC.AspNetCore.Tests;

public class DefaultRelyingPartySelectorTests
{
    [Fact]
    public async Task TestGetSelectedRelyingParty()
    {
        var _options = new MockOptionsMonitorSpidCieOptions();
        var _retriever = new RelyingPartiesHandler(_options, new DefaultRelyingPartiesRetriever());
        var _selector = new DefaultRelyingPartySelector(_retriever, new MockHttpContextAccessor(false));
        var rp = await _selector.GetSelectedRelyingParty();
        Assert.NotNull(rp);
    }
}
