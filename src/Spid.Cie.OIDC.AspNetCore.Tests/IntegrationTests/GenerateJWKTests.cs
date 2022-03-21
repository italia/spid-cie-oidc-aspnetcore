using System.Threading.Tasks;
using Xunit;

namespace Spid.Cie.OIDC.AspNetCore.Tests.IntegrationTests;

public class GenerateJWKTests
{
    [Fact]
    public async Task RegularGenerateJWK()
    {
        // Arrange
        var settings = new TestSettings();

        var server = settings.CreateTestServer();

        // Act
        var transaction = await server.SendAsync(TestServerBuilder.TestHost + "/generatejwk");

        // Assert
        Assert.NotEmpty(transaction.ResponseText);
    }


}
