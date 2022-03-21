using System.Security.Cryptography;

namespace Spid.Cie.OIDC.AspNetCore.Tests.Mocks;

internal class MockCryptoService : Helpers.CryptoService
{
    public override string ValidateJWTSignature(string jwt, RSA publicKey)
    {
        return this.DecodeJWT(jwt);
    }


    //public override string DecodeJose(string jose, RSA privateKey)
    //    => Jose.JWT.Decode(jose);
}
