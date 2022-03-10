using Spid.Cie.OIDC.AspNetCore.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Spid.Cie.OIDC.AspNetCore.Services;

internal class IdentityProvidersRetriever : IIdentityProvidersRetriever
{
    public async Task<IEnumerable<IdentityProvider>> GetIdentityProviders()
    {
        List<IdentityProvider> result = new();
        return await Task.FromResult(result);
    }
}
