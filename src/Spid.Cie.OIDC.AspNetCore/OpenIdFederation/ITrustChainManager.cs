using Spid.Cie.OIDC.AspNetCore.Models;
using System.Threading.Tasks;

namespace Spid.Cie.OIDC.AspNetCore.OpenIdFederation;

internal interface ITrustChainManager
{
    Task<IdPEntityConfiguration?> BuildTrustChain(string url);
}
