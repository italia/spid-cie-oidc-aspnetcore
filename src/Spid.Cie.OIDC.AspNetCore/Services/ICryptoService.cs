using Spid.Cie.OIDC.AspNetCore.Models;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Spid.Cie.OIDC.AspNetCore.Services;

interface ICryptoService
{
    string DecodeJWT(string jwt);

    string DecodeJWTHeader(string jwt);

    RSA GetRSAPublicKey(JsonWebKey key);

    JWKS GetJWKS(List<X509Certificate2> certificates);

    string ValidateJWTSignature(string jwt, RSA publicKey);

    JWKS GetJWKS(List<RPOpenIdCoreCertificate> certificates);

    string DecodeJose(string jose, X509Certificate2 certificate);

    string CreateJWT(X509Certificate2 certificate, object payload);

    string CreateClientAssertion(string aud, string clientId, X509Certificate2 certificate);
}
