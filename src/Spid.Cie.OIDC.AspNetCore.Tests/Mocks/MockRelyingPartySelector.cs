using Spid.Cie.OIDC.AspNetCore.Models;
using Spid.Cie.OIDC.AspNetCore.Services;
using System.Linq;
using System.Threading.Tasks;

namespace Spid.Cie.OIDC.AspNetCore.Tests.Mocks;

internal class MockRelyingPartySelector : IRelyingPartySelector
{
    private readonly bool _emptyCollection;
    private readonly bool _noKeys;

    public MockRelyingPartySelector(bool emptyCollection = false, bool noKeys = false)
    {
        _emptyCollection = emptyCollection;
        _noKeys = noKeys;
    }

    public async Task<RelyingParty?> GetSelectedRelyingParty()
    {
        return (await new MockRelyingPartiesHandler(_emptyCollection, _noKeys).GetRelyingParties()).FirstOrDefault();
    }
}
