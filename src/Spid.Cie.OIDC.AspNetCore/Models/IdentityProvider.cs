﻿using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Spid.Cie.OIDC.AspNetCore.Models;

[ExcludeFromCodeCoverage]
public abstract class IdentityProvider
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public string Uri { get; set; }
    internal abstract IdentityProviderType Type { get; }
    public string OrganizationName { get; set; }
    public string OrganizationDisplayName { get; set; }
    public string OrganizationUrl { get; set; }
    public string OrganizationLogoUrl { get; set; }
    public List<string> SupportedAcrValues { get; set; }
    internal IdPEntityConfiguration EntityConfiguration { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    public abstract string GetAcrValue(SecurityLevel securityLevel);

    public abstract IEnumerable<string> FilterRequestedClaims(List<ClaimTypes> requestedClaims);
}
