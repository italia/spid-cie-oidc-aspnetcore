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
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Threading.Tasks;

namespace Spid.Cie.OIDC.AspNetCore.Middlewares;

class FetchOpenIdFederationMiddleware
{
    readonly RequestDelegate _next;

    public FetchOpenIdFederationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context,
        IRelyingPartiesHandler rpHandler,
        IAggregatorsHandler aggHandler,
        ICryptoService cryptoService)
    {
        if (!context.Request.Path.Value!.EndsWith(SpidCieConst.FetchEndpointPath, StringComparison.InvariantCultureIgnoreCase))
        {
            await _next(context);
            return;
        }

        var uri = new Uri(UriHelper.GetEncodedUrl(context.Request))
           .GetLeftPart(UriPartial.Path)
           .Replace(SpidCieConst.FetchEndpointPath, "")
           .EnsureTrailingSlash()
           .ToString();

        string? sub = context.Request.Query.ContainsKey("sub") ? context.Request.Query["sub"] : default;
        string? iss = context.Request.Query.ContainsKey("iss") ? context.Request.Query["iss"] : uri;

        if (string.IsNullOrWhiteSpace(sub) || string.IsNullOrWhiteSpace(iss))
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            context.Response.ContentType = SpidCieConst.JsonContentType;
            await context.Response.WriteAsync(JsonSerializer.Serialize(new GenericError()
            {
                ErrorCode = ErrorCodes.invalid_request,
                ErrorDescription = "'sub' and 'iss' query parameters are mandatory"
            }));
            await context.Response.Body.FlushAsync();
            return;
        }

        var aggs = await aggHandler.GetAggregators();
        var aggregate = aggs.FirstOrDefault(r => iss.Equals(r.Id.EnsureTrailingSlash(), StringComparison.OrdinalIgnoreCase));

        if (aggregate is null)
        {
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            context.Response.ContentType = SpidCieConst.JsonContentType;
            await context.Response.WriteAsync(JsonSerializer.Serialize(new GenericError()
            {
                ErrorCode = ErrorCodes.invalid_request,
                ErrorDescription = "'iss' not found"
            }));
            await context.Response.Body.FlushAsync();
            return;
        }

        var certificate = aggregate?.OpenIdFederationCertificates?.FirstOrDefault();

        if (certificate is null)
        {
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            context.Response.ContentType = SpidCieConst.JsonContentType;
            await context.Response.WriteAsync(JsonSerializer.Serialize(new GenericError()
            {
                ErrorCode = ErrorCodes.invalid_request,
                ErrorDescription = "'iss' doesn't have a signing certificate"
            }));
            await context.Response.Body.FlushAsync();
            return;
        }

        var relyingParty = aggregate.RelyingParties.FirstOrDefault(r => sub.EnsureTrailingSlash().Equals(r.Id.EnsureTrailingSlash(), StringComparison.OrdinalIgnoreCase));

        if (relyingParty is null)
        {
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            context.Response.ContentType = SpidCieConst.JsonContentType;
            await context.Response.WriteAsync(JsonSerializer.Serialize(new GenericError()
            {
                ErrorCode = ErrorCodes.invalid_request,
                ErrorDescription = "'sub' not found"
            }));
            await context.Response.Body.FlushAsync();
            return;
        }

        if (relyingParty.TrustMarks is null || !relyingParty.TrustMarks.Any())
        {
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            context.Response.ContentType = SpidCieConst.JsonContentType;
            await context.Response.WriteAsync(JsonSerializer.Serialize(new GenericError()
            {
                ErrorCode = ErrorCodes.invalid_request,
                ErrorDescription = "Invalid TrustMarks for 'sub'"
            }));
            await context.Response.Body.FlushAsync();
            return;
        }

        var response = new EntityStatement()
        {
            ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(SpidCieConst.EntityConfigurationExpirationInMinutes),
            IssuedAt = DateTimeOffset.UtcNow,
            Issuer = iss,
            Subject = sub,
            JWKS = cryptoService.GetJWKS(new List<X509Certificate2>() { certificate }),
            MetadataPolicy = aggregate.MetadataPolicy,
            TrustMarks = relyingParty.TrustMarks,
            AuthorityHints = relyingParty.AuthorityHints,
            OpenIdRelyingParty = new SA_SpidCieOIDCConfiguration
            {
                Value = new SAJWKSValue
                {
                    JWKS = cryptoService.GetJWKS(relyingParty.OpenIdCoreCertificates)
                }
            }
        };

        string token = cryptoService.CreateJWT(certificate, response);

        context.Response.ContentType = SpidCieConst.EntityConfigurationContentType;
        await context.Response.WriteAsync(token);
        await context.Response.Body.FlushAsync();
        return;
    }
}