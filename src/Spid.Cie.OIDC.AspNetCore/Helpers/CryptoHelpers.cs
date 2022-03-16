using IdentityModel;
using JWT;
using JWT.Algorithms;
using JWT.Builder;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.IdentityModel.Tokens;
using Spid.Cie.OIDC.AspNetCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace Spid.Cie.OIDC.AspNetCore.Helpers;

internal static class CryptoHelpers
{
    internal static (RSA publicKey, RSA privateKey) GetRSAKeys(this Microsoft.IdentityModel.Tokens.JsonWebKey key)
        => (RSA.Create(new RSAParameters()
        {
            Modulus = WebEncoders.Base64UrlDecode(key.N),
            Exponent = WebEncoders.Base64UrlDecode(key.E),
        }), RSA.Create(new RSAParameters()
        {
            Modulus = WebEncoders.Base64UrlDecode(key.N),
            Exponent = WebEncoders.Base64UrlDecode(key.E),
            D = WebEncoders.Base64UrlDecode(key.D),
            P = WebEncoders.Base64UrlDecode(key.P),
            Q = WebEncoders.Base64UrlDecode(key.Q),
            DP = WebEncoders.Base64UrlDecode(key.DP),
            DQ = WebEncoders.Base64UrlDecode(key.DQ),
            InverseQ = WebEncoders.Base64UrlDecode(key.QI)
        }));

    internal static string DecodeJWT(this string jwt)
        => JwtBuilder.Create().Decode(jwt);

    internal static string DecodeJose(this string jose, RSA privateKey)
        => Jose.JWT.Decode(jose, privateKey);

    internal static string CreateJWT(RSA publicKey,
        RSA privateKey,
        Dictionary<string, object> headers,
        Dictionary<string, object> claims)
    {
        var builder = JwtBuilder.Create()
            .WithAlgorithm(new RS256Algorithm(publicKey, privateKey));

        foreach (var (key, value) in headers ?? new Dictionary<string, object>())
        {
            builder.AddHeader(key, value);
        }
        foreach (var (key, value) in claims ?? new Dictionary<string, object>())
        {
            builder.AddClaim(key, value);
        }

        return builder.Encode();
    }

    internal static JWKS GetJWKS(this JsonWebKeySet jwks)
        => new JWKS()
        {
            Keys = jwks.Keys?.Select(jsonWebKey =>
            {
                return new Models.JsonWebKey()
                {
                    kty = jsonWebKey.Kty,
                    use = jsonWebKey.Use ?? "sig",
                    kid = jsonWebKey.Kid,
                    x5t = jsonWebKey.X5t,
                    e = jsonWebKey.E,
                    n = jsonWebKey.N,
                    x5c = jsonWebKey.X5c?.Count == 0 ? Array.Empty<string>() : jsonWebKey.X5c!.ToArray(),
                    alg = jsonWebKey.Alg,
                    crv = jsonWebKey.Crv,
                    x = jsonWebKey.X,
                    y = jsonWebKey.Y
                };
            }).ToArray() ?? Array.Empty<Models.JsonWebKey>()
        };

    internal static RsaSecurityKey CreateRsaSecurityKey(int keySize = 2048)
        => new RsaSecurityKey(RSA.Create(keySize).ExportParameters(true))
        {
            KeyId = CryptoRandom.CreateUniqueId(16, CryptoRandom.OutputFormat.Hex)
        };

    internal static Models.JsonWebKey GetPublicJWK(this Microsoft.IdentityModel.Tokens.JsonWebKey jwk)
        => new Models.JsonWebKey()
        {
            kty = jwk.Kty,
            kid = jwk.Kid,
            n = jwk.N,
            e = jwk.E
        };

    internal static Models.JsonWebKey GetPrivateJWK(this Microsoft.IdentityModel.Tokens.JsonWebKey jwk)
        => new Models.JsonWebKey()
        {
            kty = jwk.Kty,
            kid = jwk.Kid,
            n = jwk.N,
            e = jwk.E,
            d = jwk.D,
            p = jwk.P,
            q = jwk.Q,
            dp = jwk.DP,
            dq = jwk.DQ,
            qi = jwk.QI,
        };

    internal static string JWTEncode(this RPEntityConfiguration entityConfiguration, Microsoft.IdentityModel.Tokens.JsonWebKey key)
    {
        (RSA publicKey, RSA privateKey) = key.GetRSAKeys();

        IJwtAlgorithm algorithm = new RS256Algorithm(publicKey, privateKey);
        IJsonSerializer serializer = new SerializationHelpers();
        IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
        IJwtEncoder encoder = new JwtEncoder(algorithm, serializer, urlEncoder);

        var exportedPrivateKey = privateKey.ExportRSAPrivateKey();
        var token = encoder.Encode(new Dictionary<string, object>()
                {
                    { SpidCieConst.Kid , key.Kid },
                    { SpidCieConst.Typ, SpidCieConst.TypValue }
                }, entityConfiguration, exportedPrivateKey);
        return token;
    }
}
