using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using Spid.Cie.OIDC.AspNetCore.Helpers;
using Spid.Cie.OIDC.AspNetCore.Models;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Spid.Cie.OIDC.AspNetCore.OpenIdFederation;

internal class JWKGeneratorMiddleware
{
    private static readonly JsonSerializerOptions _options = new JsonSerializerOptions()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
    private readonly RequestDelegate _next;

    public JWKGeneratorMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        if (!context.Request.Path.Value.EndsWith(SpidCieDefaults.JWKGeneratorPath, StringComparison.InvariantCultureIgnoreCase))
        {
            await _next(context);
            return;
        }

        var key = CryptoHelpers.CreateRsaSecurityKey();

        var jwk = JsonWebKeyConverter.ConvertFromRSASecurityKey(key);

        Models.JsonWebKey privateJwk = jwk.GetPrivateJWK();
        Models.JsonWebKey publicJwk = jwk.GetPublicJWK();

        var json = JsonSerializer.Serialize(new
        {
            PrivateJwk = JsonSerializer.Serialize(privateJwk, _options),
            PublicJwk = JsonSerializer.Serialize(publicJwk, _options)
        }, _options);

        context.Response.ContentType = SpidCieDefaults.JWKGeneratorContentType;
        await context.Response.WriteAsync(json);
        await context.Response.Body.FlushAsync();
    }
}
