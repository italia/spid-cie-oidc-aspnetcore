using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Spid.Cie.OIDC.AspNetCore.Models;
using System.Threading.Tasks;

namespace Spid.Cie.OIDC.AspNetCore.OpenIdFederation;

internal class OpenIdFederationMiddleware
{
    private readonly RequestDelegate _next;

    public OpenIdFederationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context, IOptionsSnapshot<SpidCieOptions> options/*, IServiceProvidersFactory serviceProvidersFactory*/)
    {
        // TODO: implement OIDC-Fed endpoints

        await _next(context);
    }
}
