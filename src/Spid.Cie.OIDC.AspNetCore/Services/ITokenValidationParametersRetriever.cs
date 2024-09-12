using Microsoft.IdentityModel.Tokens;
using System.Threading.Tasks;

namespace Spid.Cie.OIDC.AspNetCore.Services;

interface ITokenValidationParametersRetriever
{
    Task<TokenValidationParameters> RetrieveTokenValidationParameter();
}