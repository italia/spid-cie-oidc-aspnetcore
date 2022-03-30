using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Spid.Cie.OIDC.AspNetCore.Models;

[ExcludeFromCodeCoverage]
internal sealed class SpidCieConfiguration
{
    public bool RequestRefreshToken { get; set; } = false;

    public List<RelyingParty> RelyingParties { get; set; } = new List<RelyingParty>();

    public List<string> SpidOPs { get; set; } = new List<string>();

    public List<string> CieOPs { get; set; } = new List<string>();

    public int IdentityProvidersCacheExpirationInMinutes { get; set; }
}
