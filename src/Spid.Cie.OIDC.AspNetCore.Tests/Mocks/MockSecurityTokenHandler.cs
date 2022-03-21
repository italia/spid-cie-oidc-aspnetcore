using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Spid.Cie.OIDC.AspNetCore.Services
{
    internal class MockSecurityTokenHandler : JwtSecurityTokenHandler
    {
        public MockSecurityTokenHandler()
        {
        }
        public override ClaimsPrincipal ValidateToken(string token, TokenValidationParameters validationParameters, out SecurityToken validatedToken)
        {
            var jwtToken = ReadJwtToken(token);
            validatedToken = jwtToken;
            var identity = CreateClaimsIdentity(jwtToken, jwtToken.Issuer, validationParameters);
            if (validationParameters.SaveSigninToken)
                identity.BootstrapContext = jwtToken.RawData;

            return new ClaimsPrincipal(identity);
        }
    }
}