using Spid.Cie.OIDC.AspNetCore.Models;
using Spid.Cie.OIDC.AspNetCore.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Spid.Cie.OIDC.AspNetCore.Tests.Mocks
{
    internal class MockRelyingPartiesRetriever : IRelyingPartiesRetriever
    {
        private readonly bool _emptyCollection;

        public MockRelyingPartiesRetriever(bool emptyCollection = false)
        {
            _emptyCollection = emptyCollection;
        }

        public async Task<IEnumerable<RelyingParty>> GetRelyingParties()
        {
            await Task.CompletedTask;
            return _emptyCollection
                ? Enumerable.Empty<RelyingParty>()
                : new MockOptionsMonitorSpidCieOptions().CurrentValue.RelyingParties;
        }
    }
}
