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
        var uri = new Uri(UriHelper.GetEncodedUrl(context.Request));

        var aggrid = uri
           .GetLeftPart(UriPartial.Path)
           .Replace(SpidCieConst.FetchEndpointPath, "")
           .EnsureTrailingSlash()
           .ToString();

        string? sub = context.Request.Query.ContainsKey("sub") ? context.Request.Query["sub"] : default;
        string? iss = context.Request.Query.ContainsKey("iss") ? context.Request.Query["iss"] : aggrid;

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

        var certificate = aggregate.OpenIdFederationCertificates?.FirstOrDefault();

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

        //check trust mark and return entity statement with new trust mark
        if (!RPHaveValidTrustMark(cryptoService, relyingParty, aggregate, out var trustmark))
        {
            var aggrTrustMarkId = aggregate.TrustMarks.FirstOrDefault()?.Id ?? "";
            //if I don't have Id in aggr trust mark or Id doesn't contain a valid url 
            if ( string.IsNullOrEmpty(aggrTrustMarkId) && aggrTrustMarkId.IndexOfOccurence("/",3) == -1)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Found;
                context.Response.ContentType = SpidCieConst.JsonContentType;
                await context.Response.WriteAsync(JsonSerializer.Serialize(new GenericError()
                {
                    ErrorCode = ErrorCodes.invalid_client,
                    ErrorDescription = "Invalid TrustMarks for 'iss'"
                }));
                await context.Response.Body.FlushAsync();
                return;
            }

            var id = $"{aggrTrustMarkId.Substring(0,aggrTrustMarkId.IndexOfOccurence("/",3))}/openid_relying_party/{relyingParty.OrganizationType?.ToLower()}";

            var emission = DateTimeOffset.Now;
            var trustMark = new TrustMarkPayload() {
                Subject = relyingParty.Id,
                Issuer = aggregate.Id,
                OrganizationType = relyingParty.OrganizationType?.ToLower(),
                Id = id,
                ExpiresOn = new DateTimeOffset(emission.Year, emission.Month, emission.Day, 0, 0, 0, emission.TimeOfDay).AddYears(1),
                IssuedAt = emission,
            };


            var trustMarkDef = new TrustMarkDefinition()
            {
                Id =  id,
                Issuer = aggregate.Id,
                TrustMark = cryptoService.CreateJWT(certificate, trustMark)
            };
            
            
            var resp = new EntityStatement()
            {
                ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(SpidCieConst.EntityConfigurationExpirationInMinutes),
                IssuedAt = DateTimeOffset.UtcNow,
                Issuer = iss,
                Subject = sub,
                JWKS = cryptoService.GetJWKS(new List<X509Certificate2>() { certificate }),
                MetadataPolicy = aggregate.MetadataPolicy,
                TrustMarks = new List<TrustMarkDefinition>(){ trustMarkDef },
                AuthorityHints = relyingParty.AuthorityHints,
                OpenIdRelyingParty = new SA_SpidCieOIDCConfiguration
                {
                    Value = new SAJWKSValue
                    {
                        JWKS = cryptoService.GetJWKS(relyingParty.OpenIdCoreCertificates)
                    }
                }
            };

            string tok = cryptoService.CreateJWT(certificate, resp);

            context.Response.ContentType = SpidCieConst.EntityConfigurationContentType;
            await context.Response.WriteAsync(tok);
            await context.Response.Body.FlushAsync();
            return;
        }

        // return entity statement with actual trust mark 
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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cryptoService"></param>
    /// <param name="rp"></param>
    /// <param name="aggregate"></param>
    /// <param name="trustmark"></param>
    /// <returns> true, if RP have a valid trust mark for the aggregate. False, instead.</returns>
    private bool RPHaveValidTrustMark(ICryptoService cryptoService, RelyingParty rp, Aggregator aggregate, out TrustMarkDefinition trustmark) 
    {
        trustmark = new TrustMarkDefinition();

        if (rp.TrustMarks is null || !rp.TrustMarks.Any())
        {
            return false;
        }

        trustmark = rp.TrustMarks.Where(x => x.Issuer == aggregate.Id).FirstOrDefault() ?? new TrustMarkDefinition();

        if (string.IsNullOrEmpty(trustmark.TrustMark))
        {
            return false;
        }

        var tmp = JsonSerializer.Deserialize<TrustMarkPayload>(cryptoService.DecodeJWT(trustmark.TrustMark));

        if (tmp is null || tmp is not null && tmp.ExpiresOn <= DateTimeOffset.Now)
        {
            return false;
        }

        return true;
    }
}