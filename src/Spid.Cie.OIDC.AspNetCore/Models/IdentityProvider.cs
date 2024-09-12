using Spid.Cie.OIDC.AspNetCore.Enums;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Spid.Cie.OIDC.AspNetCore.Models;

[ExcludeFromCodeCoverage]
public abstract class IdentityProvider
{
    public string? Uri { get; set; }

    public string? OrganizationName { get; set; }

    public string? OrganizationLogoUrl { get; set; }

    internal abstract IdentityProviderTypes Type { get; }

    public List<string> SupportedAcrValues { get; set; } = new();

    internal OPEntityConfiguration? EntityConfiguration { get; set; }

    public string GetAcrValue(SecurityLevels securityLevel)
        => securityLevel == SecurityLevels.L1 && SupportedAcrValues.Contains(SpidCieConst.SpidL1) ? SpidCieConst.SpidL1 :
            securityLevel == SecurityLevels.L2 && SupportedAcrValues.Contains(SpidCieConst.SpidL2) ? SpidCieConst.SpidL2 :
            securityLevel == SecurityLevels.L3 && SupportedAcrValues.Contains(SpidCieConst.SpidL3) ? SpidCieConst.SpidL3 :
            SpidCieConst.DefaultAcr;

    public abstract IEnumerable<string> FilterRequestedClaims(List<ClaimTypes> requestedClaims);
}