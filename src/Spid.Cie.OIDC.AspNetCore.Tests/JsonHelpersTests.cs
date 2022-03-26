using Spid.Cie.OIDC.AspNetCore.Helpers;
using Xunit;

namespace Spid.Cie.OIDC.AspNetCore.Tests;

public class JsonHelpersTests
{
    [Fact]
    public void IsNullOrWhiteSpace1()
    {
        Assert.True(JsonExtensions.IsNullOrWhiteSpace(null));
    }
}
