using System.Collections.Generic;
using System.Threading.Tasks;

namespace Spid.Cie.OIDC.AspNetCore.Services;

public interface IIdentityProvidersRetriever
{
    Task<IEnumerable<string>> GetSpidIdentityProviders();
    Task<IEnumerable<string>> GetCieIdentityProviders();
}