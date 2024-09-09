using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Spid.Cie.OIDC.AspNetCore.Enums;
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

class ResolveOpenIdFederationMiddleware
{
    readonly RequestDelegate _next;

    public ResolveOpenIdFederationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context, IRelyingPartiesHandler rpHandler, IAggregatorsHandler aggHandler, ICryptoService cryptoService, ITrustChainManager trustChainManager)
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
                ErrorCode = ErrorCodes.invalid_request,
                ErrorDescription = "'sub' and 'anchor' query parameters are mandatory"
            }));
            await context.Response.Body.FlushAsync();

            return;
        }

        var uri = new Uri(UriHelper.GetEncodedUrl(context.Request))
            .GetLeftPart(UriPartial.Path)
            .Replace(SpidCieConst.ResolveEndpointPath, "")
            .EnsureTrailingSlash()
            .ToString();
        var rps = await rpHandler.GetRelyingParties();
        var aggregator = (await aggHandler.GetAggregators()).FirstOrDefault(r => uri.Equals(r.Id.EnsureTrailingSlash(), StringComparison.OrdinalIgnoreCase));
        var certificate = rps.FirstOrDefault(r => uri.Equals(r.Id.EnsureTrailingSlash(), StringComparison.OrdinalIgnoreCase))?.OpenIdFederationCertificates?.FirstOrDefault()
            ?? aggregator?.OpenIdFederationCertificates?.FirstOrDefault();

        if (certificate is not null)
        {
            var trustChain = trustChainManager.GetResolvedTrustChain<OPEntityConfiguration>(sub, anchor);

            if (trustChain != null)
            {
                var response = new OPResolveConfiguration()
                {
                    ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(SpidCieConst.EntityConfigurationExpirationInMinutes),
                    IssuedAt = DateTimeOffset.UtcNow,
                    Issuer = uri,
                    //Metadata = trustChain.OpConf.Metadata,
                    Metadata = trustChain.EntityConfiguration.Metadata,
                    Subject = sub,
                    //TrustMarks = trustChain.OpConf.TrustMarks?
                    //    .Where(t => JsonSerializer.Deserialize<IdPEntityConfiguration>(cryptoService.DecodeJWT(t.TrustMark)).ExpiresOn >= DateTimeOffset.UtcNow)
                    //    .ToList() ?? new List<TrustMarkDefinition>(),
                    TrustMarks = trustChain.EntityConfiguration.TrustMarks?
                        .Where(t => JsonSerializer.Deserialize<OPEntityConfiguration>(cryptoService.DecodeJWT(t.TrustMark)).ExpiresOn >= DateTimeOffset.UtcNow)
                        .ToList() ?? new List<TrustMarkDefinition>(),
                    TrustChain = trustChain.Chain
                };

                string token = cryptoService.CreateJWT(certificate, response);

                context.Response.ContentType = SpidCieConst.ResolveContentType;
                await context.Response.WriteAsync(token);
                await context.Response.Body.FlushAsync();

                return;
            }
            else if ((aggregator.RelyingParties.Union(rps)).Any(rp => rp.Id.EnsureTrailingSlash().Equals(sub.EnsureTrailingSlash(), StringComparison.InvariantCultureIgnoreCase)))
            {
                var rpTrustChain = trustChainManager.GetResolvedTrustChain<RPEntityConfiguration>(sub, anchor);

                if (rpTrustChain == default)
                {
                    _ = await trustChainManager.BuildRPTrustChain(sub);
                    rpTrustChain = trustChainManager.GetResolvedTrustChain<RPEntityConfiguration>(sub, anchor);
                }

                if (rpTrustChain != default)
                {
                    var response = new RPResolveConfiguration
                    {
                        ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(SpidCieConst.EntityConfigurationExpirationInMinutes),
                        IssuedAt = DateTimeOffset.UtcNow,
                        Issuer = uri,
                        Metadata = rpTrustChain.EntityConfiguration?.Metadata ?? new(),
                        Subject = sub,
                        TrustChain = rpTrustChain.Chain,
                        TrustMarks = rpTrustChain.EntityConfiguration?.TrustMarks?
                                        .Where(t => JsonSerializer.Deserialize<RPEntityConfiguration>(cryptoService.DecodeJWT(t.TrustMark))?.ExpiresOn >= DateTimeOffset.UtcNow)
                                        .ToList() ?? new()
                    };
                    string token = cryptoService.CreateJWT(certificate, response);

                    context.Response.ContentType = SpidCieConst.ResolveContentType;
                    await context.Response.WriteAsync(token);
                    await context.Response.Body.FlushAsync();

                    return;
                }
            }

            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            context.Response.ContentType = SpidCieConst.JsonContentType;
            await context.Response.WriteAsync(JsonSerializer.Serialize(new GenericError()
            {
                ErrorCode = ErrorCodes.not_found,
                ErrorDescription = "TrustChain not found"
            }));
            await context.Response.Body.FlushAsync();

            return;
        }

        context.Response.StatusCode = (int)HttpStatusCode.NotFound;
        context.Response.ContentType = SpidCieConst.JsonContentType;
        await context.Response.WriteAsync(JsonSerializer.Serialize(new GenericError()
        {
            ErrorCode = ErrorCodes.not_found,
            ErrorDescription = "'sub' not found"
        }));
        await context.Response.Body.FlushAsync();

        return;
    }
}