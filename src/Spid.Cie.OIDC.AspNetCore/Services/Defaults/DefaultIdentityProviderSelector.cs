using Microsoft.AspNetCore.Http;
using Spid.Cie.OIDC.AspNetCore.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Spid.Cie.OIDC.AspNetCore.Services.Defaults;

class DefaultIdentityProviderSelector : IIdentityProviderSelector
{
    private readonly IIdentityProvidersHandler _idpHandler;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DefaultIdentityProviderSelector(IHttpContextAccessor httpContextAccessor, IIdentityProvidersHandler idpHandler)
    {
        _idpHandler = idpHandler;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<IdentityProvider?> GetSelectedIdentityProvider()
    {
        var identityProviders = await _idpHandler.GetIdentityProviders();
        var provider = (string?)_httpContextAccessor.HttpContext!.Request.Query[SpidCieConst.IdPSelectorKey]
            ?? (string?)_httpContextAccessor.HttpContext!.Items[SpidCieConst.IdPSelectorKey];

        if (!string.IsNullOrWhiteSpace(provider))
            return identityProviders.FirstOrDefault(idp => idp.Uri.Equals(provider, System.StringComparison.InvariantCultureIgnoreCase));

        return default;
    }
}
