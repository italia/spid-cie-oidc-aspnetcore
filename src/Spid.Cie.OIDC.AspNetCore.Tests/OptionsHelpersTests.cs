using Microsoft.Extensions.Configuration;
using Spid.Cie.OIDC.AspNetCore.Helpers;
using Xunit;

namespace Spid.Cie.OIDC.AspNetCore.Tests;

public class OptionsHelpersTests
{
    public static IConfiguration InitConfiguration()
    {
        var config = new ConfigurationBuilder()
           .AddJsonFile("appsettings.test.json")
            .AddEnvironmentVariables()
            .Build();
        return config;
    }

    [Fact]
    public void EnsureTrailingSlash()
    {
        var config = InitConfiguration();
        Assert.NotNull(OptionsHelpers.CreateFromConfiguration(config));
    }

}
