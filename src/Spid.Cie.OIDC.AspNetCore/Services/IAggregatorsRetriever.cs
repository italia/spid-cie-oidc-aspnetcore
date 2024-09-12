using Spid.Cie.OIDC.AspNetCore.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Spid.Cie.OIDC.AspNetCore.Services;

public interface IAggregatorsRetriever
{
    Task<IEnumerable<Aggregator>> GetAggregators();
}