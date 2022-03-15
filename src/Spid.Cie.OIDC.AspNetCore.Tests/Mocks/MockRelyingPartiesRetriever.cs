using Spid.Cie.OIDC.AspNetCore.Models;
using Spid.Cie.OIDC.AspNetCore.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Spid.Cie.OIDC.AspNetCore.Tests.Mocks;

internal class MockRelyingPartiesRetriever : IRelyingPartiesRetriever
{
    public Task<IEnumerable<RelyingParty>> GetRelyingParties()
    {
        return Task.FromResult(Enumerable.Empty<RelyingParty>());
    }
}
