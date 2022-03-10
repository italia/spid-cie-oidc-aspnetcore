using Spid.Cie.OIDC.AspNetCore.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Spid.Cie.OIDC.AspNetCore.Services;

internal class DefaultRelyingPartiesRetriever : IRelyingPartiesRetriever
{
    public Task<List<RelyingParty>> GetRelyingParties()
    {
        return Task.FromResult(new List<RelyingParty>());
    }
}
