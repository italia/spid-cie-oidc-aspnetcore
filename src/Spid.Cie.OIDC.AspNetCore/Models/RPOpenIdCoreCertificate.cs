using Spid.Cie.OIDC.AspNetCore.Enums;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography.X509Certificates;

namespace Spid.Cie.OIDC.AspNetCore.Models
{
    [ExcludeFromCodeCoverage]
    public class RPOpenIdCoreCertificate
    {
        public string? Algorithm { get; set; }

        public KeyUsageTypes KeyUsage { get; set; }

        public X509Certificate2? Certificate { get; set; }
    }
}