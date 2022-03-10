using Microsoft.AspNetCore.Http;
using Spid.Cie.OIDC.AspNetCore.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Spid.Cie.OIDC.AspNetCore.Services;

internal class IdentityProviderSelector : IIdentityProviderSelector
{
    private readonly IIdentityProvidersRetriever _retriever;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public IdentityProviderSelector(IHttpContextAccessor httpContextAccessor, IIdentityProvidersRetriever retriever)
    {
        _retriever = retriever;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<IdentityProvider> GetSelectedIdentityProvider()
    {
        var identityProviders = await _retriever.GetIdentityProviders();
        var idpName = _httpContextAccessor.HttpContext.Request.Query["idp"];
        if (string.IsNullOrWhiteSpace(idpName))
        {
            throw new System.Exception("No 'idp' querystring parameter found in the Challenge Url");
        }
        return identityProviders.FirstOrDefault(idp => idp.Name.Equals(idpName, System.StringComparison.InvariantCultureIgnoreCase));
    }
}
