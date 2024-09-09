using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Spid.Cie.OIDC.AspNetCore.Helpers;
using Spid.Cie.OIDC.AspNetCore.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Spid.Cie.OIDC.AspNetCore.Services.Defaults;

class DefaultRelyingPartySelector : IRelyingPartySelector
{
    readonly IRelyingPartiesHandler _retriever;
    readonly IHttpContextAccessor _httpContextAccessor;

    public DefaultRelyingPartySelector(IRelyingPartiesHandler retriever,
        IHttpContextAccessor httpContextAccessor)
    {
        _retriever = retriever;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<RelyingParty?> GetSelectedRelyingParty()
    {
        var rps = await _retriever.GetRelyingParties();

        var uri = new Uri(UriHelper.GetEncodedUrl(_httpContextAccessor.HttpContext.Request))
            .GetLeftPart(UriPartial.Path)
            .Replace(SpidCieConst.JsonEntityConfigurationPath, "")
            .Replace(SpidCieConst.EntityConfigurationPath, "")
            .Replace(SpidCieConst.CallbackPath, "")
            .Replace(SpidCieConst.SignedOutCallbackPath, "")
            .Replace(SpidCieConst.RemoteSignOutPath, "")
            .EnsureTrailingSlash();

        var rp = rps.OrderByDescending(r => r.Id.Length).FirstOrDefault(r => uri.StartsWith(r.Id.EnsureTrailingSlash(), StringComparison.OrdinalIgnoreCase));

        return rp;
    }
}