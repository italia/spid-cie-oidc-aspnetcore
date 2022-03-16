using Microsoft.AspNetCore.Http;
using Spid.Cie.OIDC.AspNetCore.Helpers;
using Spid.Cie.OIDC.AspNetCore.Models;
using Spid.Cie.OIDC.AspNetCore.Services;
using System;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Spid.Cie.OIDC.AspNetCore.OpenIdFederation;

internal class RPOpenIdFederationMiddleware
{
    private static readonly JsonSerializerOptions _options = new JsonSerializerOptions()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
    private readonly RequestDelegate _next;

    public RPOpenIdFederationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context, IRelyingPartySelector rpSelector)
    {
        if (!context.Request.Path.Value!.EndsWith(SpidCieConst.EntityConfigurationPath, StringComparison.InvariantCultureIgnoreCase))
        {
            await _next(context);
            return;
        }

        var rp = await rpSelector.GetSelectedRelyingParty();
        if (rp != null)
        {
            var key = rp.OpenIdFederationJWKs.Keys?.FirstOrDefault();
            if (key is not null)
            {
                var entityConfiguration = rp.EntityConfiguration;
                string token = entityConfiguration.JWTEncode(key);

                context.Response.ContentType = SpidCieConst.EntityConfigurationContentType;
                await context.Response.WriteAsync(token);
                await context.Response.Body.FlushAsync();
                return;
            }
        }
        context.Response.StatusCode = (int)HttpStatusCode.NotFound;

    }


}
