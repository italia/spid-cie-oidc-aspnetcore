using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Spid.Cie.OIDC.AspNetCore.Enums;
using Spid.Cie.OIDC.AspNetCore.Helpers;
using Spid.Cie.OIDC.AspNetCore.Models;
using Spid.Cie.OIDC.AspNetCore.Services;
using System;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace Spid.Cie.OIDC.AspNetCore.Middlewares;

class ListOpenIdFederationMiddleware
{
    readonly RequestDelegate _next;

    public ListOpenIdFederationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context,
        IRelyingPartiesHandler rpHandler,
        IAggregatorsHandler aggHandler,
        ICryptoService cryptoService)
    {
        if (!context.Request.Path.Value!.EndsWith(SpidCieConst.ListEndpointPath, StringComparison.InvariantCultureIgnoreCase))
        {
            await _next(context);
            return;
        }

        var uri = new Uri(UriHelper.GetEncodedUrl(context.Request))
            .GetLeftPart(UriPartial.Path)
            .Replace(SpidCieConst.ListEndpointPath, "")
            .EnsureTrailingSlash()
            .ToString();

        var aggs = await aggHandler.GetAggregators();
        var aggregate = aggs.FirstOrDefault(r => uri.Equals(r.Id.EnsureTrailingSlash(), StringComparison.OrdinalIgnoreCase));

        if (aggregate is null)
        {
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            context.Response.ContentType = SpidCieConst.JsonContentType;
            await context.Response.WriteAsync(JsonSerializer.Serialize(new GenericError()
            {
                ErrorCode = ErrorCodes.invalid_request,
                ErrorDescription = "Aggregator not found"
            }));
            await context.Response.Body.FlushAsync();
            return;
        }

        var relyingParties = aggregate.RelyingParties?.Select(r => r.Id).ToList();

        context.Response.ContentType = SpidCieConst.JsonContentType;
        await context.Response.WriteAsync(JsonSerializer.Serialize(relyingParties));
        await context.Response.Body.FlushAsync();
        return;
    }
}