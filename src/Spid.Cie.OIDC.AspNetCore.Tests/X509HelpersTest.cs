using Spid.Cie.OIDC.AspNetCore.Helpers;
using System;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Xunit;

namespace Spid.Cie.OIDC.AspNetCore.Tests;

public class X509HelpersTest
{
    [Fact]
    public void GetCertificateFromStringsNullRaw()
    {
        Assert.ThrowsAny<Exception>(() => X509Helpers.GetCertificateFromStrings(null, "password"));
    }

    [Fact]
    public void GetCertificateFromStringsNullPassword()
    {
        Assert.ThrowsAny<Exception>(() => X509Helpers.GetCertificateFromStrings("raw", null));
    }

    [Fact]
    public void GetCertificateFromStringsNotValid()
    {
        Assert.ThrowsAny<Exception>(() => X509Helpers.GetCertificateFromStrings("notvalid", "password"));
    }

    [Fact]
    public void GetCertificateFromStringsNotValid2()
    {
        Assert.ThrowsAny<Exception>(() => X509Helpers.GetCertificateFromStrings(Convert.ToBase64String(Encoding.UTF8.GetBytes("test")), "password"));
    }

    [Fact]
    public void GetCertificateFromStrings()
    {
        X509Certificate2 crt = new X509Certificate2("ComuneVigata-SPID.pfx", "P@ssW0rd!", X509KeyStorageFlags.Exportable | X509KeyStorageFlags.MachineKeySet);
        var exported = Convert.ToBase64String(crt.Export(X509ContentType.Pfx, "test"));

        Assert.NotNull(X509Helpers.GetCertificateFromStrings(exported, "test"));
    }

    [Fact]
    public void GetCertificateFromFileNullPath()
    {
        Assert.ThrowsAny<Exception>(() => X509Helpers.GetCertificateFromFile(null, "password"));
    }

    [Fact]
    public void GetCertificateFromFileNullPassword()
    {
        Assert.ThrowsAny<Exception>(() => X509Helpers.GetCertificateFromFile("path", null));
    }

    [Fact]
    public void GetCertificateFromFileNotFound()
    {
        Assert.ThrowsAny<Exception>(() => X509Helpers.GetCertificateFromFile("path", "password"));
    }
}
