using Spid.Cie.OIDC.AspNetCore.Models;
using Spid.Cie.OIDC.AspNetCore.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Spid.Cie.OIDC.AspNetCore.Tests.Mocks
{
    internal class MockRelyingPartiesRetriever : IRelyingPartiesRetriever
    {
        public async Task<IEnumerable<RelyingParty>> GetRelyingParties()
        {
            await Task.CompletedTask;
            return new MockOptionsMonitorSpidCieOptions().CurrentValue.RelyingParties;
        }
    }
}
