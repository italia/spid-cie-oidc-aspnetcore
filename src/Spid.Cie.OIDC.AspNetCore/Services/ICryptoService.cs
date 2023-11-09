using Spid.Cie.OIDC.AspNetCore.Models;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Spid.Cie.OIDC.AspNetCore.Services;

internal interface ICryptoService
{
    string CreateClientAssertion(string aud, string clientId, X509Certificate2 certificate);
    string DecodeJose(string jose, X509Certificate2 certificate);
    string DecodeJWT(string jwt);
    string DecodeJWTHeader(string jwt);
    JWKS GetJWKS(List<X509Certificate2> certificates);
    RSA GetRSAPublicKey(Models.JsonWebKey key);
    string CreateJWT(X509Certificate2 certificate, object payload);
    string ValidateJWTSignature(string jwt, RSA publicKey);
}
