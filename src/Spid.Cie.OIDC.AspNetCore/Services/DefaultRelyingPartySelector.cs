using Microsoft.Extensions.Options;
using Spid.Cie.OIDC.AspNetCore.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Spid.Cie.OIDC.AspNetCore.Services;

internal class DefaultRelyingPartySelector : IRelyingPartySelector
{
    private readonly IOptionsMonitor<SpidCieOptions> _options;
    private readonly IRelyingPartiesRetriever _retriever;

    public DefaultRelyingPartySelector(IOptionsMonitor<SpidCieOptions> options, IRelyingPartiesRetriever retriever)
    {
        _options = options;
        _retriever = retriever;
    }

    public async Task<RelyingParty> GetSelectedRelyingParty()
        => _options.CurrentValue.RelyingParties
            .Union(await _retriever.GetRelyingParties())
            .FirstOrDefault();
}
