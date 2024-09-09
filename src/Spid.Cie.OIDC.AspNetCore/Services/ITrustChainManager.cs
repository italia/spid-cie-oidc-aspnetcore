using Spid.Cie.OIDC.AspNetCore.Models;
using System.Threading.Tasks;

namespace Spid.Cie.OIDC.AspNetCore.Services;

interface ITrustChainManager
{
    Task<OPEntityConfiguration?> BuildTrustChain(string url);

    Task<RPEntityConfiguration?> BuildRPTrustChain(string url);

    TrustChain<T>? GetResolvedTrustChain<T>(string sub, string anchor) where T : EntityConfiguration;
}