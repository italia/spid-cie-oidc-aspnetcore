using IdentityModel.AspNetCore.AccessTokenManagement;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Moq;
using Spid.Cie.OIDC.AspNetCore.Services;
using Spid.Cie.OIDC.AspNetCore.Tests.Mocks;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Spid.Cie.OIDC.AspNetCore.Tests;

public class AssertionConfigurationServiceTests
{
    [Fact]
    public async Task Test()
    {
        bool thrown = false;
        try
        {
            var _service = new AssertionConfigurationService(new MockHttpContextAccessor(true),
               new MockRelyingPartiesHandler(),
               new MockIdentityProvidersHandler(false),
               new IdentityModel.AspNetCore.AccessTokenManagement.UserAccessTokenManagementOptions(),
               new IdentityModel.AspNetCore.AccessTokenManagement.ClientAccessTokenManagementOptions(),
               new MockOptionsMonitorOpenIdConnectOptions(),
               Mock.Of<IAuthenticationSchemeProvider>(),
               new MockCryptoService(),
               Mock.Of<ILogger<IdentityModel.AspNetCore.AccessTokenManagement.DefaultTokenClientConfigurationService>>());

            await (typeof(DefaultTokenClientConfigurationService).InvokeMember("CreateAssertionAsync",
               System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
               Type.DefaultBinder,
               _service,
               new object[] { null }) as Task<ClientAssertion>)!;
        }
        catch (Exception ex)
        {
            thrown = true;
        }
        Assert.False(thrown);
    }

    [Fact]
    public async Task TestFail()
    {
        bool thrown = false;
        try
        {
            var _service = new AssertionConfigurationService(new MockHttpContextAccessor(true, false),
               new MockRelyingPartiesHandler(),
               new MockIdentityProvidersHandler(false),
               new IdentityModel.AspNetCore.AccessTokenManagement.UserAccessTokenManagementOptions(),
               new IdentityModel.AspNetCore.AccessTokenManagement.ClientAccessTokenManagementOptions(),
               new MockOptionsMonitorOpenIdConnectOptions(),
               null,
               new MockCryptoService(),
               Mock.Of<ILogger<IdentityModel.AspNetCore.AccessTokenManagement.DefaultTokenClientConfigurationService>>());

            await (typeof(DefaultTokenClientConfigurationService).InvokeMember("CreateAssertionAsync",
               System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
               Type.DefaultBinder,
               _service,
               new object[] { null }) as Task<ClientAssertion>)!;
        }
        catch (Exception ex)
        {
            thrown = true;
        }
        Assert.True(thrown);
    }
}
