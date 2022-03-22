using Spid.Cie.OIDC.AspNetCore.Models;
using System.Threading.Tasks;

namespace Spid.Cie.OIDC.AspNetCore.Services;

internal interface ITrustChainManager
{
    Task<IdPEntityConfiguration?> BuildTrustChain(string url);
}
