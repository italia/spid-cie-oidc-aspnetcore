using Spid.Cie.OIDC.AspNetCore.Configuration;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Spid.Cie.OIDC.AspNetCore.Tests;

public class ConfigurationManagerTests
{

    [Fact]
    public async Task TestRequestRefresh()
    {
        var confMan = new ConfigurationManager(new Mocks.MockIdentityProviderSelector(false));
        await Task.CompletedTask;
        confMan.RequestRefresh();
    }

    [Fact]
    public async Task TestGetConfigurationAsync()
    {
        var confMan = new ConfigurationManager(new Mocks.MockIdentityProviderSelector(false));
        Assert.NotNull(await confMan.GetConfigurationAsync(new System.Threading.CancellationToken()));
    }

    [Fact]
    public async Task TestGetConfigurationEmptyAsync()
    {
        var confMan = new ConfigurationManager(new Mocks.MockIdentityProviderSelector(true));
        await Assert.ThrowsAnyAsync<Exception>(async () => await confMan.GetConfigurationAsync(new System.Threading.CancellationToken()));
    }
}
