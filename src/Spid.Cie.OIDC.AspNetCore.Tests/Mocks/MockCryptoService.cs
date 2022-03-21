using System.Security.Cryptography;

namespace Spid.Cie.OIDC.AspNetCore.Tests.Mocks;

internal class MockCryptoService : Helpers.CryptoService
{
    public override string ValidateJWTSignature(string jwt, RSA publicKey)
    {
        return this.DecodeJWT(jwt);
    }
}
