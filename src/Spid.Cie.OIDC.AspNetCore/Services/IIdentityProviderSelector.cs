using Spid.Cie.OIDC.AspNetCore.Models;
using System.Threading.Tasks;

namespace Spid.Cie.OIDC.AspNetCore.Services;

internal interface IIdentityProviderSelector
{
    Task<IdentityProvider?> GetSelectedIdentityProvider();
}
