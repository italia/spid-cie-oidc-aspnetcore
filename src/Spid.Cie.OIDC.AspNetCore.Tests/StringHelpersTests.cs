using Spid.Cie.OIDC.AspNetCore.Helpers;
using Xunit;

namespace Spid.Cie.OIDC.AspNetCore.Tests;

public class StringHelpersTests
{
    [Fact]
    public void EnsureTrailingSlash()
    {
        Assert.Equal("http://127.0.0.1/", StringHelpers.EnsureTrailingSlash("http://127.0.0.1"));
    }

    [Fact]
    public void RemoveLeadingSlash()
    {
        Assert.Equal("Path", StringHelpers.RemoveLeadingSlash("/Path"));
    }
}
