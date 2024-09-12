using Spid.Cie.OIDC.AspNetCore.Resources;
using System;
using System.Security.Cryptography.X509Certificates;

namespace Spid.Cie.OIDC.AspNetCore.Helpers;

static class X509Helpers
{
    /// <summary>
    /// Get certificate from file path and password
    /// </summary>
    /// <param name="certFilePath"></param>
    /// <param name="certPassword"></param>
    /// <returns></returns>
    public static X509Certificate2 GetCertificateFromFile(string certFilePath, string certPassword)
    {
        Throw<Exception>.If(string.IsNullOrWhiteSpace(certFilePath), ErrorLocalization.CertificatePathNullOrEmpty);
        Throw<Exception>.If(string.IsNullOrWhiteSpace(certPassword), ErrorLocalization.CertificatePasswordNullOrEmpty);

        return new X509Certificate2(certFilePath,
            certPassword,
            X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);
    }

    /// <summary>
    /// Get certificate from file path and password
    /// </summary>
    /// <param name="certFilePath"></param>
    /// <param name="certPassword"></param>
    /// <returns></returns>
    public static X509Certificate2 GetCertificateFromStrings(string certificateString64, string certPassword)
    {
        Throw<Exception>.If(string.IsNullOrWhiteSpace(certificateString64), ErrorLocalization.CertificateRawStringNullOrEmpty);
        Throw<Exception>.If(string.IsNullOrWhiteSpace(certPassword), ErrorLocalization.CertificatePasswordNullOrEmpty);

        var certificateBytes = Convert.FromBase64String(certificateString64);

        return new X509Certificate2(certificateBytes, certPassword,
            X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);
    }
}