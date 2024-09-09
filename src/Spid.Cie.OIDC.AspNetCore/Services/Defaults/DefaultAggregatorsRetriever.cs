using Spid.Cie.OIDC.AspNetCore.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Spid.Cie.OIDC.AspNetCore.Services.Defaults;

class DefaultAggregatorsRetriever : IAggregatorsRetriever
{
    public async Task<IEnumerable<Aggregator>> GetAggregators()
    {
        await Task.CompletedTask;
        return Enumerable.Empty<Aggregator>();
    }
}