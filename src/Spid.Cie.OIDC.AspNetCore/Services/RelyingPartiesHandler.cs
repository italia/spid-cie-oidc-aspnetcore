using Microsoft.Extensions.Options;
using Spid.Cie.OIDC.AspNetCore.Configuration;
using Spid.Cie.OIDC.AspNetCore.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Spid.Cie.OIDC.AspNetCore.Services;

internal class RelyingPartiesHandler : IRelyingPartiesHandler
{
    private readonly IOptionsMonitor<SpidCieOptions> _options;
    private readonly IRelyingPartiesRetriever _rpRetriever;

    public RelyingPartiesHandler(IOptionsMonitor<SpidCieOptions> options,
        IRelyingPartiesRetriever rpRetriever)
    {
        _options = options;
        _rpRetriever = rpRetriever;
    }

    public async Task<IEnumerable<RelyingParty>> GetRelyingParties()
    {
        return (_options.CurrentValue.RelyingParties ?? Enumerable.Empty<RelyingParty>())
            .Union(await _rpRetriever.GetRelyingParties());
    }
}
