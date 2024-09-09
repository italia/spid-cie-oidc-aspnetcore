using Microsoft.Extensions.Options;
using Spid.Cie.OIDC.AspNetCore.Configuration;
using Spid.Cie.OIDC.AspNetCore.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Spid.Cie.OIDC.AspNetCore.Services;

class AggregatorsHandler : IAggregatorsHandler
{
    readonly IOptionsMonitor<SpidCieOptions> _options;
    readonly IAggregatorsRetriever _aggRetriever;

    public AggregatorsHandler(IOptionsMonitor<SpidCieOptions> options,
        IAggregatorsRetriever aggRetriever)
    {
        _options = options;
        _aggRetriever = aggRetriever;
    }

    public async Task<IEnumerable<Aggregator>> GetAggregators()
    {
        return _options.CurrentValue.Aggregators.Union(await _aggRetriever.GetAggregators()).ToList();
    }
}