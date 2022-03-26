using Spid.Cie.OIDC.AspNetCore.Models;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Spid.Cie.OIDC.AspNetCore.Services;

internal interface ICryptoService
{
    string CreateClientAssertion(IdentityProvider idp, string clientId, X509Certificate2 certificate);
    string CreateJWT(RSA publicKey, RSA privateKey, Dictionary<string, object> headers, Dictionary<string, object> claims);
    string DecodeJose(string jose, RSA privateKey);
    string DecodeJWT(string jwt);
    string DecodeJWTHeader(string jwt);
    Microsoft.IdentityModel.Tokens.JsonWebKey GetJsonWebKey(X509Certificate2 certificate);
    JWKS GetJWKS(X509Certificate2[] certificates);
    (RSA publicKey, RSA privateKey) GetRSAKeys(X509Certificate2 certificate);
    RSA GetRSAPublicKey(Models.JsonWebKey key);
    string JWTEncode(RPEntityConfiguration entityConfiguration, X509Certificate2 certificate);
    string ValidateJWTSignature(string jwt, RSA publicKey);
}
