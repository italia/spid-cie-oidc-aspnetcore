using Microsoft.Extensions.Logging;
using Moq;
using Spid.Cie.OIDC.AspNetCore.Services;
using Spid.Cie.OIDC.AspNetCore.Services.Defaults;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Spid.Cie.OIDC.AspNetCore.Tests;

public class TrustChainManagerTests
{
    [Fact]
    public async Task EnsureTrailingSlash()
    {
        var tcm = new TrustChainManager(Mock.Of<IHttpClientFactory>(),
            new Mocks.MockCryptoService(),
            new Mocks.MockMetadataPolicyHandler(),
            new DefaultLogPersister(Mock.Of<ILogger<DefaultLogPersister>>()),
            Mock.Of<ILogger<TrustChainManager>>());

        var result = await tcm.BuildTrustChain("http://127.0.0.1:8003/");

        Assert.Null(result);
    }

}
