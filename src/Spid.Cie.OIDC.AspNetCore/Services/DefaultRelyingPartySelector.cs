using Spid.Cie.OIDC.AspNetCore.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Spid.Cie.OIDC.AspNetCore.Services;

internal class DefaultRelyingPartySelector : IRelyingPartySelector
{
    private readonly IRelyingPartiesRetriever _retriever;

    public DefaultRelyingPartySelector(IRelyingPartiesRetriever retriever)
    {
        _retriever = retriever;
    }

    public async Task<RelyingParty> GetSelectedRelyingParty()
        => (await _retriever.GetRelyingParties()).FirstOrDefault();
}
