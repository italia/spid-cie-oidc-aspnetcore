using Microsoft.AspNetCore.Http;
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

class RPOpenIdFederationMiddleware
{
    readonly RequestDelegate _next;

    public RPOpenIdFederationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context, IRelyingPartiesHandler rpHandler, IAggregatorsHandler aggregatorsHandler, ICryptoService cryptoService)
    {
        if (!context.Request.Path.Value!.EndsWith(SpidCieConst.EntityConfigurationPath, StringComparison.InvariantCultureIgnoreCase)
            && !context.Request.Path.Value!.EndsWith(SpidCieConst.JsonEntityConfigurationPath, StringComparison.InvariantCultureIgnoreCase))
        {
            await _next(context);
            return;
        }

        var rps = (await rpHandler.GetRelyingParties())
            .Union((await aggregatorsHandler.GetAggregators()).SelectMany(a => a.RelyingParties).ToList());
        var uri = UriHelper.GetEncodedUrl(context.Request)
            .Replace(SpidCieConst.JsonEntityConfigurationPath, "")
            .Replace(SpidCieConst.EntityConfigurationPath, "")
            .EnsureTrailingSlash();
        var rp = rps.FirstOrDefault(r => uri.EnsureTrailingSlash().Equals(r.Id.EnsureTrailingSlash(), StringComparison.OrdinalIgnoreCase));

        if (rp != null)
        {
            var certificate = rp.OpenIdFederationCertificates?.FirstOrDefault();

            if (certificate is not null)
            {
                var entityConfiguration = GetEntityConfiguration(rp, cryptoService);

                if (context.Request.Path.Value!.EndsWith(SpidCieConst.EntityConfigurationPath))
                {
                    string token = cryptoService.CreateJWT(certificate, entityConfiguration);

                    context.Response.ContentType = SpidCieConst.EntityConfigurationContentType;
                    await context.Response.WriteAsync(token);
                    await context.Response.Body.FlushAsync();

                    return;
                }
                else
                {
                    context.Response.ContentType = SpidCieConst.JsonContentType;
                    await context.Response.WriteAsync(JsonSerializer.Serialize(entityConfiguration));
                    await context.Response.Body.FlushAsync();

                    return;
                }
            }
        }
        else
        {
            var aggs = await aggregatorsHandler.GetAggregators();
            var aggregate = aggs.FirstOrDefault(r => uri.EnsureTrailingSlash().Equals(r.Id.EnsureTrailingSlash(), StringComparison.OrdinalIgnoreCase));

            if (aggregate is not null)
            {
                var certificate = aggregate.OpenIdFederationCertificates?.FirstOrDefault();

                if (certificate is not null)
                {
                    var entityConfiguration = GetEntityConfiguration(aggregate, cryptoService);

                    if (context.Request.Path.Value!.EndsWith(SpidCieConst.EntityConfigurationPath))
                    {
                        string token = cryptoService.CreateJWT(certificate, entityConfiguration);

                        context.Response.ContentType = SpidCieConst.EntityConfigurationContentType;
                        await context.Response.WriteAsync(token);
                        await context.Response.Body.FlushAsync();

                        return;
                    }
                    else
                    {
                        context.Response.ContentType = SpidCieConst.JsonContentType;
                        await context.Response.WriteAsync(JsonSerializer.Serialize(entityConfiguration));
                        await context.Response.Body.FlushAsync();

                        return;
                    }
                }
            }
        }

        context.Response.StatusCode = (int)HttpStatusCode.NotFound;
    }

    private RPEntityConfiguration GetEntityConfiguration(RelyingParty rp, ICryptoService cryptoService)
    {
        return new RPEntityConfiguration()
        {
            ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(SpidCieConst.EntityConfigurationExpirationInMinutes),
            IssuedAt = DateTimeOffset.UtcNow,
            AuthorityHints = rp.AuthorityHints,
            Issuer = rp.Id,
            Subject = rp.Id,
            TrustMarks = rp.TrustMarks,
            JWKS = cryptoService.GetJWKS(rp.OpenIdFederationCertificates),
            Metadata = new RPMetadata_SpidCieOIDCConfiguration()
            {
                OpenIdRelyingParty = new RP_SpidCieOIDCConfiguration()
                {
                    ClientId = rp.Id,
                    ClientName = rp.Name,
                    GrantTypes = rp.LongSessionsEnabled
                        ? new() { SpidCieConst.AuthorizationCode, SpidCieConst.RefreshToken }
                        : new() { SpidCieConst.AuthorizationCode },
                    JWKS = cryptoService.GetJWKS(rp.OpenIdCoreCertificates),
                    RedirectUris = rp.RedirectUris,
                    ResponseTypes = new() { SpidCieConst.ResponseType }
                },
                FederationEntity = new RP_SpidCieOIDCFederationEntity()
                {
                    Contacts = rp.Contacts,
                    HomepageUri = rp.HomepageUri,
                    LogoUri = rp.LogoUri,
                    OrganizationName = rp.OrganizationName,
                    PolicyUri = rp.PolicyUri,
                    FederationResolveEndpoint = $"{rp.Id.EnsureTrailingSlash()}{SpidCieConst.ResolveEndpointPath}"
                }
            }
        };
    }

    private SAEntityConfiguration GetEntityConfiguration(Aggregator agg, ICryptoService cryptoService)
    {
        return new SAEntityConfiguration()
        {
            ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(SpidCieConst.EntityConfigurationExpirationInMinutes),
            IssuedAt = DateTimeOffset.UtcNow,
            AuthorityHints = agg.AuthorityHints,
            Issuer = agg.Id,
            Subject = agg.Id,
            TrustMarks = agg.TrustMarks,
            JWKS = cryptoService.GetJWKS(agg.OpenIdFederationCertificates),
            Metadata = new SAMetadata_SpidCieOIDCConfiguration()
            {
                FederationEntity = new SA_SpidCieOIDCFederationEntity()
                {
                    Contacts = agg.Contacts,
                    HomepageUri = agg.HomepageUri,
                    LogoUri = agg.LogoUri,
                    OrganizationName = agg.OrganizationName,
                    PolicyUri = agg.PolicyUri,
                    FederationResolveEndpoint = $"{agg.Id.EnsureTrailingSlash()}{SpidCieConst.ResolveEndpointPath}",
                    FederationFetchEndpoint = $"{agg.Id.EnsureTrailingSlash()}{SpidCieConst.FetchEndpointPath}",
                    FederationListEndpoint = $"{agg.Id.EnsureTrailingSlash()}{SpidCieConst.ListEndpointPath}",
                    FederationTrustMarkStatusEndpoint = $"{agg.Id.EnsureTrailingSlash()}{SpidCieConst.TrustMarkStatusEndpointPath}"
                }
            }
        };
    }
}