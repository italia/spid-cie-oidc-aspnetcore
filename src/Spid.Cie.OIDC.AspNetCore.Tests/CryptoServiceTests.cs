using Spid.Cie.OIDC.AspNetCore.Services;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Xunit;
using static Spid.Cie.OIDC.AspNetCore.Tests.Mocks.TestSettings;

namespace Spid.Cie.OIDC.AspNetCore.Tests;

public class CryptoServiceTests
{
    private readonly X509Certificate2 certificate;

    public CryptoServiceTests()
    {
        certificate = new X509Certificate2("ComuneVigata-SPID.pfx", "P@ssW0rd!");
    }

    [Fact]
    public async void CreateJWT()
    {
        var service = new CryptoService();
        var result = service.CreateJWT(certificate, new Dictionary<string, string>() { { "test", "test" } });
        Assert.NotNull(result);
    }

    [Fact]
    public async void DecodeJWT()
    {
        var resourceName = "Spid.Cie.OIDC.AspNetCore.Tests.IntegrationTests.jwtOP.json";
        using var stream = typeof(MockBackchannel).Assembly.GetManifestResourceStream(resourceName);
        using var reader = new StreamReader(stream);
        var body = await reader.ReadToEndAsync();

        var service = new CryptoService();
        var result = service.DecodeJWT(body);
        Assert.NotNull(result);
    }

    [Fact]
    public async void DecodeJWTHeader()
    {
        var resourceName = "Spid.Cie.OIDC.AspNetCore.Tests.IntegrationTests.jwtOP.json";
        using var stream = typeof(MockBackchannel).Assembly.GetManifestResourceStream(resourceName);
        using var reader = new StreamReader(stream);
        var body = await reader.ReadToEndAsync();

        var service = new CryptoService();
        var result = service.DecodeJWTHeader(body);
        Assert.NotNull(result);
    }

    [Fact]
    public async void DecodeJose()
    {
        var resourceName = "Spid.Cie.OIDC.AspNetCore.Tests.IntegrationTests.userInfoResponse.jose";
        using var stream = typeof(MockBackchannel).Assembly.GetManifestResourceStream(resourceName);
        using var reader = new StreamReader(stream);
        var body = await reader.ReadToEndAsync();

        var service = new CryptoService();
        var result = service.DecodeJose(body, certificate);
        Assert.NotNull(result);
    }

    [Fact]
    public async void CreateClientAssertion()
    {
        var service = new CryptoService();
        var idp = (await new Mocks.MockIdentityProvidersHandler().GetIdentityProviders()).FirstOrDefault();
        var result = service.CreateClientAssertion(idp!.EntityConfiguration.Metadata.OpenIdProvider!.TokenEndpoint, "http://127.0.0.1:5000/", certificate);
        Assert.NotNull(result);
    }

    [Fact]
    public async void ValidateJWTSignature()
    {
        var service = new CryptoService();
        var jwt = service.CreateJWT(certificate, new Dictionary<string, string>() { { "test", "test" } });
        Assert.NotNull(jwt);
        var result = service.ValidateJWTSignature(jwt, certificate.GetRSAPublicKey()!);
        Assert.NotNull(result);
    }
}
