using Microsoft.AspNetCore.Http;
using Spid.Cie.OIDC.AspNetCore.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Spid.Cie.OIDC.AspNetCore.Services;

internal class DefaultIdentityProviderSelector : IIdentityProviderSelector
{
    private readonly IIdentityProvidersRetriever _retriever;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DefaultIdentityProviderSelector(IHttpContextAccessor httpContextAccessor, IIdentityProvidersRetriever retriever)
    {
        _retriever = retriever;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<IdentityProvider> GetSelectedIdentityProvider()
    {
        var identityProviders = await _retriever.GetIdentityProviders();
        var provider = (string)_httpContextAccessor.HttpContext.Request.Query[SpidCieDefaults.IdPSelectorKey]
            ?? (string)_httpContextAccessor.HttpContext.Items[SpidCieDefaults.IdPSelectorKey];
        if (!string.IsNullOrWhiteSpace(provider))
        {
            return identityProviders.FirstOrDefault(idp => idp.Name.Equals(provider, System.StringComparison.InvariantCultureIgnoreCase));
        }
        return default;
    }
}
