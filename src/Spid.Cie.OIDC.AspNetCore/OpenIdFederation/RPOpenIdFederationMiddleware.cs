using Microsoft.AspNetCore.Http;
using Spid.Cie.OIDC.AspNetCore.Helpers;
using Spid.Cie.OIDC.AspNetCore.Models;
using Spid.Cie.OIDC.AspNetCore.Services;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Spid.Cie.OIDC.AspNetCore.OpenIdFederation;

internal class RPOpenIdFederationMiddleware
{
    private readonly RequestDelegate _next;

    public RPOpenIdFederationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context, IRelyingPartySelector rpSelector, ICryptoService cryptoService)
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
                var entityConfiguration = GetEntityConfiguration(rp, cryptoService);
                string token = cryptoService.JWTEncode(entityConfiguration, key);

                context.Response.ContentType = SpidCieConst.EntityConfigurationContentType;
                await context.Response.WriteAsync(token);
                await context.Response.Body.FlushAsync();
                return;
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
            JWKS = cryptoService.GetJWKS(rp.OpenIdFederationJWKs),
            Metadata = new RPMetadata_SpidCieOIDCConfiguration()
            {
                OpenIdRelyingParty = new RP_SpidCieOIDCConfiguration()
                {
                    ClientName = rp.ClientName,
                    Contacts = rp.Contacts,
                    GrantTypes = rp.LongSessionsEnabled
                        ? new[] { SpidCieConst.AuthorizationCode, SpidCieConst.RefreshToken }
                        : new[] { SpidCieConst.AuthorizationCode },
                    JWKS = cryptoService.GetJWKS(rp.OpenIdCoreJWKs),
                    RedirectUris = rp.RedirectUris,
                    ResponseTypes = new[] { SpidCieConst.ResponseType }
                }
            }
        };
    }
}
