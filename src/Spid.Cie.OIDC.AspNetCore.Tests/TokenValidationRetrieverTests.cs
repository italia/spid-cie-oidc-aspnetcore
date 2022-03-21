using Spid.Cie.OIDC.AspNetCore.Services;
using Spid.Cie.OIDC.AspNetCore.Tests.Mocks;
using System.Threading.Tasks;
using Xunit;

namespace Spid.Cie.OIDC.AspNetCore.Tests;

public class TokenValidationRetrieverTests
{
    private readonly ITokenValidationParametersRetriever _retriever;

    public TokenValidationRetrieverTests()
    {
        var _idpSelector = new MockIdentityProviderSelector(false);
        var _rpSelector = new MockRelyingPartySelector();
        _retriever = new TokenValidationParametersRetriever(_idpSelector, _rpSelector);
    }

    [Fact]
    public async Task TestGetSelectedRelyingParty()
    {
        var rp = await _retriever.RetrieveTokenValidationParameter();
        Assert.NotNull(rp);
    }
}
