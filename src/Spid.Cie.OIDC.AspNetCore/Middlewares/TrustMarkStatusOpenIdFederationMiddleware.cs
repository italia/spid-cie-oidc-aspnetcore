﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Spid.Cie.OIDC.AspNetCore.Helpers;
using Spid.Cie.OIDC.AspNetCore.Models;
using Spid.Cie.OIDC.AspNetCore.Services;
using System;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace Spid.Cie.OIDC.AspNetCore.Middlewares;

internal class TrustMarkStatusOpenIdFederationMiddleware
{
    private readonly RequestDelegate _next;

    public TrustMarkStatusOpenIdFederationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context,
        IRelyingPartiesHandler rpHandler,
        IAggregatorsHandler aggHandler,
        ICryptoService cryptoService)
    {
        if (!(context.Request.Path.Value!.EndsWith(SpidCieConst.TrustMarkStatusEndpointPath, StringComparison.InvariantCultureIgnoreCase)
                && context.Request.Method.Equals(HttpMethods.Post, StringComparison.OrdinalIgnoreCase)))
        {
            await _next(context);
            return;
        }

        var uri = new Uri(UriHelper.GetEncodedUrl(context.Request))
            .GetLeftPart(UriPartial.Path)
            .Replace(SpidCieConst.TrustMarkStatusEndpointPath, "")
            .EnsureTrailingSlash()
            .ToString();

        string? sub = context.Request.Form.ContainsKey("sub") ? context.Request.Form["sub"] : default;
        string? id = context.Request.Form.ContainsKey("id") ? context.Request.Form["id"] : default;

        if (string.IsNullOrWhiteSpace(sub) || string.IsNullOrWhiteSpace(id))
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            context.Response.ContentType = SpidCieConst.JsonContentType;
            await context.Response.WriteAsync(JsonSerializer.Serialize(new GenericError()
            {
                ErrorCode = ErrorCode.invalid_request,
                ErrorDescription = $"'{nameof(sub)}' and '{nameof(id)}' query parameters are mandatory"
            }));
            await context.Response.Body.FlushAsync();
            return;
        }

        var aggs = await aggHandler.GetAggregators();

        var relyingParty = aggs
            .SelectMany(s => s.RelyingParties)
            .FirstOrDefault(r => sub.EnsureTrailingSlash().Equals(r.Id.EnsureTrailingSlash(), StringComparison.OrdinalIgnoreCase));
        if (relyingParty is null)
        {
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            context.Response.ContentType = SpidCieConst.JsonContentType;
            await context.Response.WriteAsync(JsonSerializer.Serialize(new GenericError()
            {
                ErrorCode = ErrorCode.invalid_request,
                ErrorDescription = "'sub' not found"
            }));
            await context.Response.Body.FlushAsync();
            return;
        }

        bool active = relyingParty.TrustMarks?
            .Where(t => t.Id.Equals(id, StringComparison.OrdinalIgnoreCase))
            .Select(t => JsonSerializer.Deserialize<TrustMarkPayload>(cryptoService.DecodeJWT(t.TrustMark)))
            .Where(t => t is not null)
            .Any() ?? false;

        context.Response.ContentType = SpidCieConst.JsonContentType;
        await context.Response.WriteAsync(JsonSerializer.Serialize(new { active }));
        await context.Response.Body.FlushAsync();
        return;
    }
}
