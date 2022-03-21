using Spid.Cie.OIDC.AspNetCore.Models;
using Spid.Cie.OIDC.AspNetCore.Services;
using System.Linq;
using System.Threading.Tasks;

namespace Spid.Cie.OIDC.AspNetCore.Tests.Mocks;

internal class MockRelyingPartySelector : IRelyingPartySelector
{
    public async Task<RelyingParty?> GetSelectedRelyingParty()
    {
        return (await new MockRelyingPartiesRetriever().GetRelyingParties()).FirstOrDefault();
    }
}
