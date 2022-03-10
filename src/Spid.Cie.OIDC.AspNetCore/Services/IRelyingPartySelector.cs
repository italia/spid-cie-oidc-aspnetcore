using Spid.Cie.OIDC.AspNetCore.Models;
using System.Threading.Tasks;

namespace Spid.Cie.OIDC.AspNetCore.Services;

public interface IRelyingPartySelector
{
    Task<RelyingParty> GetSelectedRelyingParty();
}
