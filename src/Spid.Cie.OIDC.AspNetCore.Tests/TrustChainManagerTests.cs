using Microsoft.Extensions.Logging;
using Moq;
using Spid.Cie.OIDC.AspNetCore.Services;
using Spid.Cie.OIDC.AspNetCore.Services.Defaults;
using System;
using System.Net.Http;
using Xunit;

namespace Spid.Cie.OIDC.AspNetCore.Tests;

public class TrustChainManagerTests
{
    [Fact]
    public void EnsureTrailingSlash()
    {
        var tcm = new TrustChainManager(Mock.Of<IHttpClientFactory>(),
            new Mocks.MockCryptoService(),
            new Mocks.MockMetadataPolicyHandler(),
            new DefaultLogPersister(Mock.Of<ILogger<DefaultLogPersister>>()),
            Mock.Of<ILogger<TrustChainManager>>());
        Assert.ThrowsAnyAsync<Exception>(async () => await tcm.BuildTrustChain("http://127.0.0.1:8003/"));
    }

}
