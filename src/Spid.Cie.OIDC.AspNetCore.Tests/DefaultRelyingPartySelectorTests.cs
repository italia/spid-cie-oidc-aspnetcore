using Spid.Cie.OIDC.AspNetCore.Services;
using Spid.Cie.OIDC.AspNetCore.Tests.Mocks;
using System.Threading.Tasks;
using Xunit;

namespace Spid.Cie.OIDC.AspNetCore.Tests;

public class DefaultRelyingPartySelectorTests
{
    private readonly IRelyingPartySelector _selector;

    public DefaultRelyingPartySelectorTests()
    {
        var _options = new MockOptionsMonitorSpidCieOptions();
        var _retriever = new DefaultRelyingPartiesRetriever(_options);
        _selector = new DefaultRelyingPartySelector(_retriever);
    }

    [Fact]
    public async Task TestGetSelectedRelyingParty()
    {
        var rp = await _selector.GetSelectedRelyingParty();
        Assert.NotNull(rp);
    }
}
