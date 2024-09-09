using Microsoft.Extensions.Logging;
using Moq;
using Spid.Cie.OIDC.AspNetCore.Configuration;
using Spid.Cie.OIDC.AspNetCore.Services.Defaults;
using Spid.Cie.OIDC.AspNetCore.Tests.Mocks;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using static Spid.Cie.OIDC.AspNetCore.Tests.Mocks.TestSettings;

namespace Spid.Cie.OIDC.AspNetCore.Tests;

public class CustomHttpClientHandlerTests
{
    [Fact]
    public async Task DecodeJoseResponseOK()
    {
        var handler = new CustomHttpClientHandler(new MockRelyingPartySelector(),
            new DefaultLogPersister(Mock.Of<ILogger<DefaultLogPersister>>()),
            new MockCryptoService(), new MockAggregatorsHandler(), new MockHttpContextAccessor(false));

        var resourceName = "Spid.Cie.OIDC.AspNetCore.Tests.IntegrationTests.userInfoResponse.jose";
        using var stream = typeof(MockBackchannel).Assembly.GetManifestResourceStream(resourceName);
        using var reader = new StreamReader(stream);
        var body = await reader.ReadToEndAsync();

        var response = new HttpResponseMessage();
        response.Content = new StringContent(body, Encoding.UTF8, "application/jose");
        Assert.NotNull(await handler.DecodeJoseResponse(response));
    }

    [Fact]
    public async Task DecodeJoseResponseNoContent()
    {
        var handler = new CustomHttpClientHandler(new MockRelyingPartySelector(),
            new DefaultLogPersister(Mock.Of<ILogger<DefaultLogPersister>>()),
            new MockCryptoService(), new MockAggregatorsHandler(), new MockHttpContextAccessor(false));
        var response = new HttpResponseMessage();
        await Assert.ThrowsAnyAsync<Exception>(() => handler.DecodeJoseResponse(response));
    }

    [Fact]
    public async Task DecodeJoseResponseNoRP()
    {
        var handler = new CustomHttpClientHandler(new MockRelyingPartySelector(true),
            new DefaultLogPersister(Mock.Of<ILogger<DefaultLogPersister>>()),
            new MockCryptoService(), new MockAggregatorsHandler(), new MockHttpContextAccessor(false));
        var resourceName = "Spid.Cie.OIDC.AspNetCore.Tests.IntegrationTests.userInfoResponse.jose";
        using var stream = typeof(MockBackchannel).Assembly.GetManifestResourceStream(resourceName);
        using var reader = new StreamReader(stream);
        var body = await reader.ReadToEndAsync();

        var response = new HttpResponseMessage();
        response.Content = new StringContent(body, Encoding.UTF8, "application/jose");
        await Assert.ThrowsAnyAsync<Exception>(() => handler.DecodeJoseResponse(response));
    }

    [Fact]
    public async Task DecodeJoseResponseNoEmptyContent()
    {
        var handler = new CustomHttpClientHandler(new MockRelyingPartySelector(),
            new DefaultLogPersister(Mock.Of<ILogger<DefaultLogPersister>>()),
            new MockCryptoService(), new MockAggregatorsHandler(), new MockHttpContextAccessor(false));
        var response = new HttpResponseMessage();
        response.Content = new StringContent(String.Empty, Encoding.UTF8, "application/jose");
        await Assert.ThrowsAnyAsync<Exception>(() => handler.DecodeJoseResponse(response));
    }

    [Fact]
    public async Task DecodeJoseResponseNoKeys()
    {
        var handler = new CustomHttpClientHandler(new MockRelyingPartySelector(false, true),
            new DefaultLogPersister(Mock.Of<ILogger<DefaultLogPersister>>()),
            new MockCryptoService(), new MockAggregatorsHandler(), new MockHttpContextAccessor(false));
        var resourceName = "Spid.Cie.OIDC.AspNetCore.Tests.IntegrationTests.userInfoResponse.jose";
        using var stream = typeof(MockBackchannel).Assembly.GetManifestResourceStream(resourceName);
        using var reader = new StreamReader(stream);
        var body = await reader.ReadToEndAsync();

        var response = new HttpResponseMessage();
        response.Content = new StringContent(body, Encoding.UTF8, "application/jose");
        await Assert.ThrowsAnyAsync<Exception>(() => handler.DecodeJoseResponse(response));
    }

    [Fact]
    public async Task DecodeJoseResponseWrongContent()
    {
        var handler = new CustomHttpClientHandler(new MockRelyingPartySelector(),
            new DefaultLogPersister(Mock.Of<ILogger<DefaultLogPersister>>()),
            new MockCryptoService(), new MockAggregatorsHandler(), new MockHttpContextAccessor(false));
        var resourceName = "Spid.Cie.OIDC.AspNetCore.Tests.IntegrationTests.jwtOP.json";
        using var stream = typeof(MockBackchannel).Assembly.GetManifestResourceStream(resourceName);
        using var reader = new StreamReader(stream);
        var body = await reader.ReadToEndAsync();

        var response = new HttpResponseMessage();
        response.Content = new StringContent(body, Encoding.UTF8, "application/jose");
        await Assert.ThrowsAnyAsync<Exception>(() => handler.DecodeJoseResponse(response));
    }

    [Fact]
    public async Task DecodeJoseResponseWrongContentType()
    {
        var handler = new CustomHttpClientHandler(new MockRelyingPartySelector(),
            new DefaultLogPersister(Mock.Of<ILogger<DefaultLogPersister>>()),
            new MockCryptoService(), new MockAggregatorsHandler(), new MockHttpContextAccessor(false));

        var resourceName = "Spid.Cie.OIDC.AspNetCore.Tests.IntegrationTests.jwtOP.json";
        using var stream = typeof(MockBackchannel).Assembly.GetManifestResourceStream(resourceName);
        using var reader = new StreamReader(stream);
        var body = await reader.ReadToEndAsync();

        var response = new HttpResponseMessage();
        response.Content = new StringContent(body, Encoding.UTF8, "application/json");
        Assert.NotNull(await handler.DecodeJoseResponse(response));
    }

    [Fact]
    public async Task SendAsync()
    {
        var handler = new CustomHttpClientHandler(new MockRelyingPartySelector(true),
            new DefaultLogPersister(Mock.Of<ILogger<DefaultLogPersister>>()),
            new MockCryptoService(), new MockAggregatorsHandler(), new MockHttpContextAccessor(false));
        var request = new HttpRequestMessage();
        request.Content = new StringContent(string.Empty);
        request.RequestUri = new Uri("http://127.0.0.1");

        bool thrown = false;
        try
        {
            await (typeof(HttpMessageHandler).InvokeMember("SendAsync",
               System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
               Type.DefaultBinder,
               handler,
               new object[] { request, CancellationToken.None }) as Task<HttpResponseMessage>)!;
        }
        catch (Exception)
        {
            thrown = true;
        }
        Assert.True(thrown);
    }
}
