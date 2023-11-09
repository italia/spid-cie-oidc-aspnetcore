using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Spid.Cie.OIDC.AspNetCore.Helpers;
using Spid.Cie.OIDC.AspNetCore.Models;
using Spid.Cie.OIDC.AspNetCore.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace Spid.Cie.OIDC.AspNetCore.Middlewares;

internal class ResolveOpenIdFederationMiddleware
{
    private readonly RequestDelegate _next;

    public ResolveOpenIdFederationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context,
        IRelyingPartiesHandler rpHandler,
        IAggregatorsHandler aggHandler,
        ICryptoService cryptoService,
        ITrustChainManager trustChainManager)
    {
        if (!context.Request.Path.Value!.EndsWith(SpidCieConst.ResolveEndpointPath, StringComparison.InvariantCultureIgnoreCase))
        {
            await _next(context);
            return;
        }

        string? sub = context.Request.Query.ContainsKey("sub") ? context.Request.Query["sub"] : default;
        string? anchor = context.Request.Query.ContainsKey("anchor") ? context.Request.Query["anchor"] : default;

        if (string.IsNullOrWhiteSpace(sub) || string.IsNullOrWhiteSpace(anchor))
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            context.Response.ContentType = SpidCieConst.JsonContentType;
            await context.Response.WriteAsync(JsonSerializer.Serialize(new GenericError()
            {
                ErrorCode = ErrorCode.invalid_request,
                ErrorDescription = "'sub' and 'anchor' query parameters are mandatory"
            }));
            await context.Response.Body.FlushAsync();
            return;
        }

        var rps = await rpHandler.GetRelyingParties();
        var aggs = await aggHandler.GetAggregators();
        var uri = new Uri(UriHelper.GetEncodedUrl(context.Request))
            .GetLeftPart(UriPartial.Path)
            .Replace(SpidCieConst.ResolveEndpointPath, "")
            .EnsureTrailingSlash()
            .ToString();

        var certificate = rps.FirstOrDefault(r => uri.Equals(r.Id.EnsureTrailingSlash(), StringComparison.OrdinalIgnoreCase))?.OpenIdFederationCertificates?.FirstOrDefault()
            ?? aggs.FirstOrDefault(r => uri.Equals(r.Id.EnsureTrailingSlash(), StringComparison.OrdinalIgnoreCase))?.OpenIdFederationCertificates?.FirstOrDefault();
        if (certificate is not null)
        {
            var trustChain = trustChainManager.GetResolvedTrustChain(sub, anchor);
            if (trustChain != null)
            {
                var response = new ResolveConfiguration()
                {
                    ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(SpidCieConst.EntityConfigurationExpirationInMinutes),
                    IssuedAt = DateTimeOffset.UtcNow,
                    Issuer = uri,
                    Metadata = trustChain.OpConf.Metadata,
                    Subject = sub,
                    TrustMarks = trustChain.OpConf.TrustMarks?
                        .Where(t => JsonSerializer.Deserialize<IdPEntityConfiguration>(cryptoService.DecodeJWT(t.TrustMark)).ExpiresOn >= DateTimeOffset.UtcNow)
                        .ToList() ?? new List<TrustMarkDefinition>(),
                    TrustChain = trustChain.Chain
                };

                string token = cryptoService.CreateJWT(certificate, response);

                context.Response.ContentType = SpidCieConst.ResolveContentType;
                await context.Response.WriteAsync(token);
                await context.Response.Body.FlushAsync();
                return;
            }

            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            context.Response.ContentType = SpidCieConst.JsonContentType;
            await context.Response.WriteAsync(JsonSerializer.Serialize(new GenericError()
            {
                ErrorCode = ErrorCode.not_found,
                ErrorDescription = "TrustChain not found"
            }));
            await context.Response.Body.FlushAsync();
            return;
        }

        context.Response.StatusCode = (int)HttpStatusCode.NotFound;
        context.Response.ContentType = SpidCieConst.JsonContentType;
        await context.Response.WriteAsync(JsonSerializer.Serialize(new GenericError()
        {
            ErrorCode = ErrorCode.not_found,
            ErrorDescription = "'sub' not found"
        }));
        await context.Response.Body.FlushAsync();
        return;
    }
}
