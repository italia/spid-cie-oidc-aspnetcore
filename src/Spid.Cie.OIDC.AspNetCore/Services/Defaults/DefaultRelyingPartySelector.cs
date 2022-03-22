using Spid.Cie.OIDC.AspNetCore.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Spid.Cie.OIDC.AspNetCore.Services.Defaults;

internal class DefaultRelyingPartySelector : IRelyingPartySelector
{
    private readonly IRelyingPartiesHandler _retriever;

    public DefaultRelyingPartySelector(IRelyingPartiesHandler retriever)
    {
        _retriever = retriever;
    }

    public async Task<RelyingParty?> GetSelectedRelyingParty()
        => (await _retriever.GetRelyingParties()).FirstOrDefault();
}
