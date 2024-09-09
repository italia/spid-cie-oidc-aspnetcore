using Spid.Cie.OIDC.AspNetCore.Enums;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography.X509Certificates;

namespace Spid.Cie.OIDC.AspNetCore.Models;

[ExcludeFromCodeCoverage]
public sealed class RelyingParty
{
    public string? Id { get; set; }

    public string? Name { get; set; }

    public string? LogoUri { get; set; }

    public string? PolicyUri { get; set; }

    public string? HomepageUri { get; set; }

    public string? OrganizationType { get; set; }

    public string? OrganizationName { get; set; }

    public bool LongSessionsEnabled { get; set; }

    public SecurityLevels SecurityLevel { get; set; }

    public List<string> Contacts { get; set; } = new();

    public List<string> RedirectUris { get; set; } = new();

    public List<string> AuthorityHints { get; set; } = new();

    public List<ClaimTypes> RequestedClaims { get; set; } = new();

    public List<TrustMarkDefinition> TrustMarks { get; set; } = new();

    public List<X509Certificate2> OpenIdFederationCertificates { get; set; } = new();

    public List<RPOpenIdCoreCertificate> OpenIdCoreCertificates { get; set; } = new();
}