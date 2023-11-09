using Spid.Cie.OIDC.AspNetCore.Models;
using Spid.Cie.OIDC.AspNetCore.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Spid.Cie.OIDC.AspNetCore.Tests.Mocks
{
    internal class MockAggregatorsHandler : IAggregatorsHandler
    {
        private readonly bool _emptyCollection;
        private readonly bool _noKeys;

        public MockAggregatorsHandler(bool emptyCollection = false, bool noKeys = false)
        {
            _emptyCollection = emptyCollection;
            _noKeys = noKeys;
        }

        public async Task<IEnumerable<Aggregator>> GetAggregators()
        {
            await Task.CompletedTask;
            var aggs = _emptyCollection
                ? Enumerable.Empty<Aggregator>()
                : new MockOptionsMonitorSpidCieOptions().CurrentValue.Aggregators;
            if (_noKeys)
            {
                foreach (var agg in aggs)
                {
                    agg.OpenIdFederationCertificates = null;
                }
            }
            return aggs;

        }
    }
}
