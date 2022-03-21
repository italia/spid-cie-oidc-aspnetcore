using Microsoft.Extensions.Logging;
using Moq;
using Spid.Cie.OIDC.AspNetCore.Configuration;
using Spid.Cie.OIDC.AspNetCore.Logging;
using Spid.Cie.OIDC.AspNetCore.Tests.Mocks;
using System.Threading.Tasks;
using Xunit;

namespace Spid.Cie.OIDC.AspNetCore.Tests;

public class CustomHttpClientHandlerTests
{
    [Fact]
    public async Task DecodeJoseResponse1()
    {
        var handler = new CustomHttpClientHandler(new MockRelyingPartySelector(),
            new DefaultLogPersister(Mock.Of<ILogger<DefaultLogPersister>>()),
            new MockCryptoService());
        var response = new System.Net.Http.HttpResponseMessage();
        Assert.Equal(await handler.DecodeJoseResponse(response), response);
    }

    [Fact]
    public async Task DecodeJoseResponse2()
    {
        var handler = new CustomHttpClientHandler(new MockRelyingPartySelector(true),
            new DefaultLogPersister(Mock.Of<ILogger<DefaultLogPersister>>()),
            new MockCryptoService());
        var response = new System.Net.Http.HttpResponseMessage();
        Assert.Equal(await handler.DecodeJoseResponse(response), response);
    }

}
