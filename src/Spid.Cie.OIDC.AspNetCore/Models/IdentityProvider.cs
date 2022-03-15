using System.Collections.Generic;

namespace Spid.Cie.OIDC.AspNetCore.Models;

public abstract class IdentityProvider
{
    public string Name { get; set; }
    public abstract IdentityProviderType Type { get; }
    public string OrganizationName { get; set; }
    public string OrganizationDisplayName { get; set; }
    public string MetadataAddress { get; set; }
    public string OrganizationUrl { get; set; }
    public string OrganizationLogoUrl { get; set; }
    public string[] SupportedAcrValues { get; set; }

    internal IdPEntityConfiguration EntityConfiguration { get; set; }

    public abstract string GetAcrValue(SecurityLevel securityLevel);

    public abstract IEnumerable<string> FilterRequestedClaims(ClaimTypes[] requestedClaims);
}
