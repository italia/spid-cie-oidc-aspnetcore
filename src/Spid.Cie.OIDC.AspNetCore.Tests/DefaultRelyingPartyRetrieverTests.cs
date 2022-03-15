using Spid.Cie.OIDC.AspNetCore.Services;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Spid.Cie.OIDC.AspNetCore.Tests;

public class DefaultRelyingPartyRetrieverTests
{
    private readonly IRelyingPartiesRetriever _retriever;

    public DefaultRelyingPartyRetrieverTests()
    {
        this._retriever = new DefaultRelyingPartiesRetriever();
    }

    [Fact]
    public async Task TestGetRelyingParties()
    {
        var rp = await _retriever.GetRelyingParties();
        Assert.NotNull(rp);
        Assert.True(rp.Count() == 0);
    }
}
