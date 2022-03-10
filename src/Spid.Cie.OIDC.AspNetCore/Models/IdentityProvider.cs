namespace Spid.Cie.OIDC.AspNetCore.Models
{
    public sealed class IdentityProvider
    {
        public string Name { get; set; }
        public IdentityProviderType Type { get; set; }
        public string OrganizationName { get; set; }
        public string OrganizationDisplayName { get; set; }
        public string OrganizationUrlMetadata { get; set; }
        public string OrganizationUrl { get; set; }
        public string OrganizationLogoUrl { get; set; }
        public string SingleSignOnServiceUrl { get; set; }
        public string SingleSignOutServiceUrl { get; set; }
        public int SecurityLevel { get; set; }
        public string SupportedAcrValues { get; set; }
    }
}
