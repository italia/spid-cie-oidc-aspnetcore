using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Spid.Cie.OIDC.AspNetCore.Models;
using Spid.Cie.OIDC.AspNetCore.Services;
using System;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace Spid.Cie.OIDC.AspNetCore.Middlewares;

internal class RPOpenIdFederationMiddleware
{
    private readonly RequestDelegate _next;

    public RPOpenIdFederationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context, IRelyingPartiesHandler rpHandler, ICryptoService cryptoService)
    {
        if (!context.Request.Path.Value!.EndsWith(SpidCieConst.EntityConfigurationPath, StringComparison.InvariantCultureIgnoreCase)
            && !context.Request.Path.Value!.EndsWith(SpidCieConst.JsonEntityConfigurationPath, StringComparison.InvariantCultureIgnoreCase))
        {
            await _next(context);
            return;
        }

        var rps = await rpHandler.GetRelyingParties();
        var uri = new Uri(UriHelper.GetEncodedUrl(context.Request)
            .Replace(SpidCieConst.JsonEntityConfigurationPath, "")
            .Replace(SpidCieConst.EntityConfigurationPath, ""));

        var rp = rps.FirstOrDefault(r => uri.Equals(new Uri(r.ClientId)));
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
                    context.Response.ContentType = SpidCieConst.JsonEntityConfigurationContentType;
                    await context.Response.WriteAsync(JsonSerializer.Serialize(entityConfiguration));
                    await context.Response.Body.FlushAsync();
                    return;
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
            Issuer = rp.Issuer,
            Subject = rp.ClientId,
            TrustMarks = rp.TrustMarks,
            JWKS = cryptoService.GetJWKS(rp.OpenIdFederationCertificates),
            Metadata = new RPMetadata_SpidCieOIDCConfiguration()
            {
                OpenIdRelyingParty = new RP_SpidCieOIDCConfiguration()
                {
                    ClientName = rp.ClientName,
                    Contacts = rp.Contacts,
                    GrantTypes = rp.LongSessionsEnabled
                        ? new[] { SpidCieConst.AuthorizationCode, SpidCieConst.RefreshToken }
                        : new[] { SpidCieConst.AuthorizationCode },
                    JWKS = cryptoService.GetJWKS(rp.OpenIdCoreCertificates),
                    RedirectUris = rp.RedirectUris,
                    ResponseTypes = new[] { SpidCieConst.ResponseType }
                }
            }
        };
    }
}
