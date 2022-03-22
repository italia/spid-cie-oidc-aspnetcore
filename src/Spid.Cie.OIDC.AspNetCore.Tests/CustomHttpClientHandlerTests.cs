using Microsoft.Extensions.Logging;
using Moq;
using Spid.Cie.OIDC.AspNetCore.Configuration;
using Spid.Cie.OIDC.AspNetCore.Services.Defaults;
using Spid.Cie.OIDC.AspNetCore.Tests.Mocks;
using System;
using System.Net.Http;
using System.Threading;
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
        var response = new HttpResponseMessage();
        await Assert.ThrowsAnyAsync<Exception>(() => handler.DecodeJoseResponse(response));
    }

    [Fact]
    public async Task DecodeJoseResponse2()
    {
        var handler = new CustomHttpClientHandler(new MockRelyingPartySelector(true),
            new DefaultLogPersister(Mock.Of<ILogger<DefaultLogPersister>>()),
            new MockCryptoService());
        var response = new HttpResponseMessage();
        await Assert.ThrowsAnyAsync<Exception>(() => handler.DecodeJoseResponse(response));
    }

    [Fact]
    public async Task SendAsync()
    {
        var handler = new CustomHttpClientHandler(new MockRelyingPartySelector(true),
            new DefaultLogPersister(Mock.Of<ILogger<DefaultLogPersister>>()),
            new MockCryptoService());
        var request = new HttpRequestMessage();
        request.Content = new StringContent(string.Empty);

        bool thrown = false;
        try
        {
            await (typeof(HttpMessageHandler).InvokeMember("SendAsync",
               System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
               Type.DefaultBinder,
               handler,
               new object[] { request, CancellationToken.None }) as Task<HttpResponseMessage>)!;
        }
        catch (Exception ex)
        {
            thrown = true;
        }
        Assert.True(thrown);
    }
}
