using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;

namespace Spid.Cie.OIDC.AspNetCore.Models;

[ExcludeFromCodeCoverage]
public sealed class Aggregator
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public string Id { get; set; }
    public string Name { get; set; }
    public string OrganizationName { get; set; }
    public string HomepageUri { get; set; }
    public string LogoUri { get; set; }
    public string PolicyUri { get; set; }
    public List<string> Contacts { get; set; } = new();
    public List<string> AuthorityHints { get; set; } = new();
    public List<TrustMarkDefinition> TrustMarks { get; set; } = new();
    public List<X509Certificate2> OpenIdFederationCertificates { get; set; } = new();
    public JsonDocument MetadataPolicy { get; set; }
    public List<RelyingParty> RelyingParties { get; set; } = new();
    public string OrganizationType { get; set; }
    public string Extension { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}
