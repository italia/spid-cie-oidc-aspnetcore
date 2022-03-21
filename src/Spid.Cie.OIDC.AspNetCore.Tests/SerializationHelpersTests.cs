using Spid.Cie.OIDC.AspNetCore.Helpers;
using System.Linq;
using Xunit;

namespace Spid.Cie.OIDC.AspNetCore.Tests;

public class SerializationHelpersTests
{
    [Fact]
    public void Serialize()
    {
        Assert.Equal("[0,1]", new SerializationHelpers().Serialize(new int[] { 0, 1 }));
    }

    [Fact]
    public void Deserialize()
    {
        Assert.True(new SerializationHelpers().Deserialize<int[]>("[0,1]").SequenceEqual(new int[] { 0, 1 }));
    }

}
