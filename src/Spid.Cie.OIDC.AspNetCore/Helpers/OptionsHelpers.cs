using Microsoft.Extensions.Configuration;
using Spid.Cie.OIDC.AspNetCore.Models;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace Spid.Cie.OIDC.AspNetCore.Helpers;

internal static class OptionsHelpers
{
    internal static SpidCieConfiguration CreateFromConfiguration(IConfiguration configuration)
    {
        var section = configuration.GetSection("SpidCie");
        var options = new SpidCieConfiguration();

        options.IdentityProvidersCacheExpirationInMinutes = section.GetValue<int?>("IdentityProvidersCacheExpirationInMinutes") ?? 10;
        options.RequestRefreshToken = section.GetValue<bool?>("RequestRefreshToken") ?? false;

        options.SpidOPs = section.GetSection("SpidOPs").Get<List<string>?>() ?? new List<string>();
        options.CieOPs = section.GetSection("CieOPs").Get<List<string>?>() ?? new List<string>();

        foreach (var relyingPartySection in section
               .GetSection("RelyingParties")
               .GetChildren()
               .ToList())
        {
            var relyingParty = new RelyingParty();
            var securityLevel = relyingPartySection.GetValue<int?>("SecurityLevel") ?? 2;
            relyingParty.SecurityLevel = securityLevel == 1 ? SecurityLevel.L1 : securityLevel == 3 ? SecurityLevel.L3 : SecurityLevel.L2;
            relyingParty.Contacts = relyingPartySection.GetSection("Contacts").Get<List<string>?>() ?? new List<string>();
            relyingParty.RedirectUris = relyingPartySection.GetSection("RedirectUris").Get<List<string>?>() ?? new List<string>();
            relyingParty.Issuer = relyingPartySection.GetValue<string?>("Issuer") ?? string.Empty;
            relyingParty.ClientId = relyingPartySection.GetValue<string?>("ClientId") ?? string.Empty;
            relyingParty.ClientName = relyingPartySection.GetValue<string?>("ClientName") ?? string.Empty;

            relyingParty.RequestedClaims.AddRange((relyingPartySection.GetSection("RequestedClaims").Get<List<string>?>() ?? new List<string>())
                .Select(c => ClaimTypes.FromName(c)).ToList());

            relyingParty.LongSessionsEnabled = relyingPartySection.GetValue<bool?>("LongSessionsEnabled") ?? false;
            relyingParty.AuthorityHints = relyingPartySection.GetSection("AuthorityHints").Get<List<string>?>() ?? new List<string>();

            foreach (var trustMarksSection in relyingPartySection.GetSection("TrustMarks")
                .GetChildren()
                .ToList())
            {
                relyingParty.TrustMarks.Add(new TrustMarkDefinition()
                {
                    Id = trustMarksSection.GetValue<string>("Id"),
                    Issuer = trustMarksSection.GetValue<string>("Issuer"),
                    TrustMark = trustMarksSection.GetValue<string>("TrustMark")
                });
            }

            foreach (var openIdFederationCertificatesSection in relyingPartySection.GetSection("OpenIdFederationCertificates")
                .GetChildren()
                .ToList())
            {
                relyingParty.OpenIdFederationCertificates.Add(GetCertificate(openIdFederationCertificatesSection));
            }

            foreach (var openIdCoreCertificatesSection in relyingPartySection.GetSection("OpenIdCoreCertificates")
                .GetChildren()
                .ToList())
            {
                relyingParty.OpenIdCoreCertificates.Add(GetCertificate(openIdCoreCertificatesSection));
            }

            options.RelyingParties.Add(relyingParty);
        }
        return options;
    }

    private static X509Certificate2 GetCertificate(IConfigurationSection openIdFederationCertificatesSection)
    {
        var certificateSource = openIdFederationCertificatesSection.GetValue<string>("Source");
        if (certificateSource.Equals("File", System.StringComparison.OrdinalIgnoreCase))
        {
            var storeConfiguration = openIdFederationCertificatesSection.GetSection("File");
            var path = storeConfiguration.GetValue<string>("Path");
            var password = storeConfiguration.GetValue<string>("Password");
            return X509Helpers.GetCertificateFromFile(path, password);
        }
        else if (certificateSource.Equals("Raw", System.StringComparison.OrdinalIgnoreCase))
        {
            var storeConfiguration = openIdFederationCertificatesSection.GetSection("Raw");
            var certificate = storeConfiguration.GetValue<string>("Certificate");
            var key = storeConfiguration.GetValue<string>("Password");
            return X509Helpers.GetCertificateFromStrings(certificate, key);
        }

        throw new System.Exception($"Invalid Certificate Source {certificateSource}");
    }
}
