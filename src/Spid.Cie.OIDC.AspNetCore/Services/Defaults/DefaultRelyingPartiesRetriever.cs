using Spid.Cie.OIDC.AspNetCore.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Spid.Cie.OIDC.AspNetCore.Services.Defaults;

class DefaultRelyingPartiesRetriever : IRelyingPartiesRetriever
{
    public async Task<IEnumerable<RelyingParty>> GetRelyingParties()
    {
        await Task.CompletedTask;
        return Enumerable.Empty<RelyingParty>();
    }
}