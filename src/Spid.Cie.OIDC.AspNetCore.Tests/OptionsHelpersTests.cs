using Microsoft.Extensions.Configuration;
using Moq;
using Spid.Cie.OIDC.AspNetCore.Helpers;
using Xunit;

namespace Spid.Cie.OIDC.AspNetCore.Tests;

public class OptionsHelpersTests
{
    [Fact]
    public void EnsureTrailingSlash()
    {
        Assert.NotNull(OptionsHelpers.CreateFromConfiguration(Mock.Of<IConfiguration>()));
    }

}
