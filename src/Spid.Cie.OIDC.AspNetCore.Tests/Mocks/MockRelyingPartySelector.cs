using Spid.Cie.OIDC.AspNetCore.Models;
using Spid.Cie.OIDC.AspNetCore.Services;
using System.Linq;
using System.Threading.Tasks;

namespace Spid.Cie.OIDC.AspNetCore.Tests.Mocks;

internal class MockRelyingPartySelector : IRelyingPartySelector
{
    private readonly bool _emptyCollection;

    public MockRelyingPartySelector(bool emptyCollection = false)
    {
        _emptyCollection = emptyCollection;
    }

    public async Task<RelyingParty?> GetSelectedRelyingParty()
    {
        return (await new MockRelyingPartiesRetriever(_emptyCollection).GetRelyingParties()).FirstOrDefault();
    }
}
