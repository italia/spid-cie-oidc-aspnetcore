using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography.X509Certificates;

namespace Spid.Cie.OIDC.AspNetCore.Models;

[ExcludeFromCodeCoverage]
public sealed class RelyingParty
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public string ClientId { get; set; }
    public string ClientName { get; set; }
    public SecurityLevel SecurityLevel { get; set; }
    public List<string> AuthorityHints { get; set; } = new();
    public string Issuer { get; set; }
    public List<TrustMarkDefinition> TrustMarks { get; set; } = new();
    public List<X509Certificate2> OpenIdFederationCertificates { get; set; } = new();
    public List<X509Certificate2> OpenIdCoreCertificates { get; set; } = new();
    public List<string> Contacts { get; set; } = new();
    public bool LongSessionsEnabled { get; set; }
    public List<string> RedirectUris { get; set; } = new();
    public List<ClaimTypes> RequestedClaims { get; set; } = new();
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}
