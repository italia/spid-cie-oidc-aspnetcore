using IdentityModel;
using JWT;
using JWT.Algorithms;
using JWT.Builder;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.IdentityModel.Tokens;
using Spid.Cie.OIDC.AspNetCore.Helpers;
using Spid.Cie.OIDC.AspNetCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Spid.Cie.OIDC.AspNetCore.Services;

internal class CryptoService : ICryptoService
{
    public RSA GetRSAPublicKey(Models.JsonWebKey key)
        => RSA.Create(new RSAParameters()
        {
            Modulus = WebEncoders.Base64UrlDecode(key.n),
            Exponent = WebEncoders.Base64UrlDecode(key.e),
        });

    public string DecodeJWTHeader(string jwt)
        => JwtBuilder.Create().DecodeHeader(jwt);

    public string DecodeJWT(string jwt)
        => JwtBuilder.Create().Decode(jwt);

    public virtual string ValidateJWTSignature(string jwt, RSA publicKey)
        => JwtBuilder.Create()
            .WithAlgorithm(new RS256Algorithm(publicKey))
            .MustVerifySignature()
            .Decode(jwt);

    public virtual string DecodeJose(string jose, X509Certificate2 certificate)
        => Jose.JWT.Decode(jose, GetPrivateKey(certificate));

    public Microsoft.IdentityModel.Tokens.JsonWebKey GetJsonWebKey(X509Certificate2 certificate)
        => JsonWebKeyConverter.ConvertFromX509SecurityKey(new X509SecurityKey(certificate));

    public JWKS GetJWKS(X509Certificate2[] certificates)
    {
        JWKS result = new JWKS();
        foreach (var certificate in certificates)
        {
            var parameters = certificate.GetRSAPublicKey()!.ExportParameters(false);
            var jsonWebKey = GetJsonWebKey(certificate);
            var exponent = Base64Url.Encode(parameters.Exponent);
            var modulus = Base64Url.Encode(parameters.Modulus);
            result.Keys.Add(new Models.JsonWebKey()
            {
                kty = jsonWebKey.Kty,
                use = jsonWebKey.Use ?? "sig",
                kid = jsonWebKey.Kid,
                x5t = jsonWebKey.X5t,
                e = exponent,
                n = modulus,
                x5c = jsonWebKey.X5c.ToArray(),
                alg = jsonWebKey.Alg ?? "RS256",
            });
        }
        return result;
    }

    public string CreateJWT(X509Certificate2 certificate, object payload)
    {
        (RSA publicKey, RSA privateKey) = CryptoService.GetRSAKeys(certificate);

        IJwtAlgorithm algorithm = new RS256Algorithm(publicKey, privateKey);
        IJsonSerializer serializer = new CustomJsonSerializer();
        IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
        IJwtEncoder encoder = new JwtEncoder(algorithm, serializer, urlEncoder);

        var key = GetJsonWebKey(certificate);

        var token = encoder.Encode(new Dictionary<string, object>()
        {
            { SpidCieConst.Kid , key.Kid },
            { SpidCieConst.Typ, SpidCieConst.TypValue }
        }, payload, null);
        return token;
    }

    public string CreateClientAssertion(IdentityProvider idp,
            string clientId,
            X509Certificate2 certificate)
        => CreateJWT(certificate,
            new Dictionary<string, object>() {
                { SpidCieConst.Iss, clientId! },
                { SpidCieConst.Sub, clientId! },
                { SpidCieConst.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
                { SpidCieConst.Exp, DateTimeOffset.UtcNow.AddMinutes(SpidCieConst.EntityConfigurationExpirationInMinutes).ToUnixTimeSeconds() },
                { SpidCieConst.Aud, new string[] { idp!.EntityConfiguration.Metadata.OpenIdProvider!.TokenEndpoint } },
                { SpidCieConst.Jti, Guid.NewGuid().ToString() }
            });

    private static (RSA publicKey, RSA privateKey) GetRSAKeys(X509Certificate2 certificate)
        => (certificate.GetRSAPublicKey()!, certificate.GetRSAPrivateKey()!);

    private static RSA GetPrivateKey(X509Certificate2 certificate)
        => certificate.GetRSAPrivateKey()!;

}
