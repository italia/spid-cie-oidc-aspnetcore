using Spid.Cie.OIDC.AspNetCore.Services;
using Spid.Cie.OIDC.AspNetCore.Tests.Mocks;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Spid.Cie.OIDC.AspNetCore.Tests;

public class TokenValidationRetrieverTests
{
    [Fact]
    public async Task TestGetSelectedRelyingParty()
    {
        var _idpSelector = new MockIdentityProviderSelector(false);
        var _rpSelector = new MockRelyingPartySelector();
        var _retriever = new TokenValidationParametersRetriever(_idpSelector,
            _rpSelector, new MockAggregatorsHandler(), new MockHttpContextAccessor(false));
        var rp = await _retriever.RetrieveTokenValidationParameter();
        Assert.NotNull(rp);
    }

    [Fact]
    public async Task TestGetSelectedRelyingPartyEmptyRP()
    {
        var _idpSelector = new MockIdentityProviderSelector(false);
        var _rpSelector = new MockRelyingPartySelector(true);
        var _retriever = new TokenValidationParametersRetriever(_idpSelector, _rpSelector, new MockAggregatorsHandler(), new MockHttpContextAccessor(false));
        await Assert.ThrowsAnyAsync<Exception>(async () => await _retriever.RetrieveTokenValidationParameter());
    }

    [Fact]
    public async Task TestGetSelectedRelyingPartyEmptyIdP()
    {
        var _idpSelector = new MockIdentityProviderSelector(true);
        var _rpSelector = new MockRelyingPartySelector(false);
        var _retriever = new TokenValidationParametersRetriever(_idpSelector, _rpSelector, new MockAggregatorsHandler(), new MockHttpContextAccessor(false));
        await Assert.ThrowsAnyAsync<Exception>(async () => await _retriever.RetrieveTokenValidationParameter());
    }
}
