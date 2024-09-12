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

class CryptoService : ICryptoService
{
    static readonly STJSerializer _jsonSerializer = new STJSerializer();
    static readonly UtcDateTimeProvider _utcDateTimeProvider = new UtcDateTimeProvider();
    static readonly JwtBase64UrlEncoder _jwtBase64UrlEncoder = new JwtBase64UrlEncoder();
    static readonly JwtValidator _jwtValidator = new JwtValidator(_jsonSerializer, _utcDateTimeProvider, new ValidationParameters()
    {
        ValidateSignature = true,
        ValidateExpirationTime = false,
        ValidateIssuedTime = false,
        TimeMargin = 300
    }, _jwtBase64UrlEncoder);
    static readonly IJwtDecoder _decoder = new JwtDecoder(_jsonSerializer, _jwtBase64UrlEncoder);

    public RSA GetRSAPublicKey(Models.JsonWebKey key)
        => RSA.Create(new RSAParameters()
        {
            Modulus = WebEncoders.Base64UrlDecode(key.N),
            Exponent = WebEncoders.Base64UrlDecode(key.E),
        });

    public string DecodeJWTHeader(string jwt)
        => _decoder.DecodeHeader(jwt);

    public string DecodeJWT(string jwt)
        => _decoder.Decode(jwt, false);

    public virtual string ValidateJWTSignature(string jwt, RSA publicKey)
        => JwtBuilder.Create()
            .WithJsonSerializer(_jsonSerializer)
            .WithValidator(_jwtValidator)
            .WithUrlEncoder(_jwtBase64UrlEncoder)
            .WithAlgorithm(new RS256Algorithm(publicKey))
            .MustVerifySignature()
            .Decode(jwt);

    public virtual string DecodeJose(string jose, X509Certificate2 certificate)
        => Jose.JWT.Decode(jose, certificate.GetRSAPrivateKey()!).Replace("\"", "");

    public JWKS GetJWKS(List<X509Certificate2> certificates)
        => new JWKS()
        {
            Keys = certificates.Select(c =>
            {
                var parameters = c.GetRSAPublicKey()!.ExportParameters(false);
                var jsonWebKey = GetJsonWebKey(c);
                var exponent = WebEncoders.Base64UrlEncode(parameters.Exponent!);
                var modulus = WebEncoders.Base64UrlEncode(parameters.Modulus!);

                return new Models.JsonWebKey()
                {
                    Kty = jsonWebKey.Kty,
                    Use = jsonWebKey.Use ?? "sig",
                    Kid = jsonWebKey.Kid,
                    //X5t = jsonWebKey.X5t,
                    E = exponent,
                    N = modulus,
                    //X5c = jsonWebKey.X5c.ToList(),
                    Alg = jsonWebKey.Alg ?? "RS256",
                };
            }).ToList()
        };

    public JWKS GetJWKS(List<RPOpenIdCoreCertificate> certificates)
        => new JWKS()
        {
            Keys = certificates.Select(c =>
            {
                var parameters = c.Certificate.GetRSAPublicKey()!.ExportParameters(false);
                var jsonWebKey = GetJsonWebKey(c.Certificate);
                var exponent = WebEncoders.Base64UrlEncode(parameters.Exponent!);
                var modulus = WebEncoders.Base64UrlEncode(parameters.Modulus!);

                return new Models.JsonWebKey()
                {
                    Kty = jsonWebKey.Kty,
                    Use = c.KeyUsage == Enums.KeyUsageTypes.Signature ? "sig" : c.KeyUsage == Enums.KeyUsageTypes.Encryption ? "enc" : default,
                    Kid = jsonWebKey.Kid,
                    //X5t = jsonWebKey.X5t,
                    E = exponent,
                    N = modulus,
                    //X5c = jsonWebKey.X5c.ToList(),
                    Alg = c.Algorithm
                };
            }).ToList()
        };

    public string CreateJWT(X509Certificate2 certificate, object payload)
    {
        var key = GetJsonWebKey(certificate);
        RSA publicKey = certificate.GetRSAPublicKey()!;
        RSA privateKey = certificate.GetRSAPrivateKey()!;
        IJwtEncoder encoder = new JwtEncoder(new RS256Algorithm(publicKey, privateKey), _jsonSerializer, _jwtBase64UrlEncoder);
        var token = encoder.Encode(new Dictionary<string, object>()
        {
            { SpidCieConst.Kid , key.Kid },
            { SpidCieConst.Typ, SpidCieConst.TypValue }
        }, payload, null);

        return token;
    }

    public string CreateClientAssertion(string aud, string clientId, X509Certificate2 certificate)
        => CreateJWT(certificate,
            new Dictionary<string, object>() {
                { SpidCieConst.Iss, clientId! },
                { SpidCieConst.Sub, clientId! },
                { SpidCieConst.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
                { SpidCieConst.Exp, DateTimeOffset.UtcNow.AddMinutes(SpidCieConst.EntityConfigurationExpirationInMinutes).ToUnixTimeSeconds() },
                { SpidCieConst.Aud, new string[] { aud } },
                { SpidCieConst.Jti, Guid.NewGuid().ToString() }
            });

    private static Microsoft.IdentityModel.Tokens.JsonWebKey GetJsonWebKey(X509Certificate2 certificate)
        => JsonWebKeyConverter.ConvertFromX509SecurityKey(new X509SecurityKey(certificate));
}