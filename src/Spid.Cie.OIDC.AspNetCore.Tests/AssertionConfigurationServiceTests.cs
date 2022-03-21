using Microsoft.Extensions.Logging;
using Moq;
using Spid.Cie.OIDC.AspNetCore.Models;
using Spid.Cie.OIDC.AspNetCore.Services;
using Spid.Cie.OIDC.AspNetCore.Tests.Mocks;
using System.Threading.Tasks;
using Xunit;

namespace Spid.Cie.OIDC.AspNetCore.Tests;

public class AssertionConfigurationServiceTests
{
    private readonly AssertionConfigurationService _service;
    public AssertionConfigurationServiceTests()
    {
        _service = new AssertionConfigurationService(new MockHttpContextAccessor(true),
            new MockRelyingPartiesRetriever(),
            new MockIdentityProvidersRetriever(false),
            new IdentityModel.AspNetCore.AccessTokenManagement.UserAccessTokenManagementOptions(),
            new IdentityModel.AspNetCore.AccessTokenManagement.ClientAccessTokenManagementOptions(),
            new MockOptionsMonitorOpenIdConnectOptions(),
            null,
            new MockCryptoService(),
            Mock.Of<ILogger<IdentityModel.AspNetCore.AccessTokenManagement.DefaultTokenClientConfigurationService>>());
    }

    [Fact]
    public async Task Test()
    {
        var tokenRequest = await _service.GetRefreshTokenRequestAsync(new IdentityModel.AspNetCore.AccessTokenManagement.UserAccessTokenParameters()
        {
            ChallengeScheme = SpidCieConst.AuthenticationScheme
        });
        Assert.NotNull(tokenRequest);
    }
}
