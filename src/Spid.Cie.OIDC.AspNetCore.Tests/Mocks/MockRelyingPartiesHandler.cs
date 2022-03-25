using Spid.Cie.OIDC.AspNetCore.Models;
using Spid.Cie.OIDC.AspNetCore.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Spid.Cie.OIDC.AspNetCore.Tests.Mocks
{
    internal class MockRelyingPartiesHandler : IRelyingPartiesHandler
    {
        private readonly bool _emptyCollection;
        private readonly bool _noKeys;

        public MockRelyingPartiesHandler(bool emptyCollection = false, bool noKeys = false)
        {
            _emptyCollection = emptyCollection;
            _noKeys = noKeys;
        }

        public async Task<IEnumerable<RelyingParty>> GetRelyingParties()
        {
            await Task.CompletedTask;
            var rps = _emptyCollection
                ? Enumerable.Empty<RelyingParty>()
                : new MockOptionsMonitorSpidCieOptions().CurrentValue.RelyingParties;
            if (_noKeys)
            {
                foreach (var rp in rps)
                {
                    rp.OpenIdCoreJWKs = null;
                    rp.OpenIdFederationJWKs = null;
                }
            }
            return rps;
        }
    }
}
