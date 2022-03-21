using Spid.Cie.OIDC.AspNetCore.Tests.Mocks;
using System.Threading.Tasks;
using Xunit;

namespace Spid.Cie.OIDC.AspNetCore.Tests.IntegrationTests;

public class OpenIdFederationTests
{
    [Fact]
    public async Task RegularGenerateJWK()
    {
        // Arrange
        var settings = new TestSettings();

        var server = settings.CreateTestServer();

        // Act
        var transaction = await server.SendAsync(TestServerBuilder.TestHost + "/.well-known/openid-federation");

        // Assert
        Assert.NotNull(new MockCryptoService().DecodeJWT(transaction.ResponseText));
    }

    [Fact]
    public async Task RegularGenerateJWKEmpty()
    {
        // Arrange
        var settings = new TestSettings(o => o.RelyingParties.Clear());

        var server = settings.CreateTestServer();

        // Act
        var transaction = await server.SendAsync(TestServerBuilder.TestHost + "/.well-known/openid-federation");

        // Assert
        Assert.True(transaction.Response.StatusCode == System.Net.HttpStatusCode.NotFound);
    }
}
