using Microsoft.Extensions.Configuration;
using Spid.Cie.OIDC.AspNetCore.Configuration;
using System.Collections.Generic;
using Xunit;

namespace Spid.Cie.OIDC.AspNetCore.Tests;

public class SpidCieOptionsTests
{
    [Fact]
    public void LoadFromConfiguration()
    {
        var inMemorySettings = new Dictionary<string, string> {
                {"TopLevelKey", "TopLevelValue"},
                {"SectionName:SomeKey", "SectionValue"},
            };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
        new SpidCieOptions().LoadFromConfiguration(configuration);
    }


}
