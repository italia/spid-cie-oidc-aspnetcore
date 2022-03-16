using Microsoft.Extensions.Options;
using Spid.Cie.OIDC.AspNetCore.Configuration;
using Spid.Cie.OIDC.AspNetCore.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Spid.Cie.OIDC.AspNetCore.Services;

internal class DefaultRelyingPartiesRetriever : IRelyingPartiesRetriever
{
    private readonly IOptionsMonitor<SpidCieOptions> _options;

    public DefaultRelyingPartiesRetriever(IOptionsMonitor<SpidCieOptions> options)
    {
        _options = options;
    }

    public async Task<IEnumerable<RelyingParty>> GetRelyingParties()
    {
        await Task.CompletedTask;
        return _options.CurrentValue.RelyingParties;
    }
}
