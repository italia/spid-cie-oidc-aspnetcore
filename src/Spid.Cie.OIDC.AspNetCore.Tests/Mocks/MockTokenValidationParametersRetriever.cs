using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Threading.Tasks;

namespace Spid.Cie.OIDC.AspNetCore.Services;

internal class MockTokenValidationParametersRetriever : ITokenValidationParametersRetriever
{

    public MockTokenValidationParametersRetriever()
    {
    }

    public async Task<TokenValidationParameters> RetrieveTokenValidationParameter()
    {
        await Task.CompletedTask;

        return new TokenValidationParameters
        {
            NameClaimType = SpidCieConst.Sub,
            ClockSkew = TimeSpan.FromMinutes(5),
            RequireSignedTokens = false,
            RequireExpirationTime = false,
            ValidateLifetime = false,
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateActor = false,
            ValidateIssuerSigningKey = false,
            ValidateTokenReplay = false,
            SignatureValidator = (string token, TokenValidationParameters validationParameters) =>
            {
                var handler = new JsonWebTokenHandler();
                var securityToken = handler.ReadToken(token);
                return securityToken;
            }
        };
    }
}
