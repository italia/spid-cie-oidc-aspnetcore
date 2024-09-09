using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Spid.Cie.OIDC.AspNetCore.Services.Defaults;

class DefaultIdentityProvidersRetriever : IIdentityProvidersRetriever
{
    public async Task<IEnumerable<string>> GetSpidIdentityProviders() => await Task.FromResult(Enumerable.Empty<string>());

    public async Task<IEnumerable<string>> GetCieIdentityProviders() => await Task.FromResult(Enumerable.Empty<string>());
}