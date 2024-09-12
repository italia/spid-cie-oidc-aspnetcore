using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Xunit;

namespace Spid.Cie.OIDC.AspNetCore.Tests.IntegrationTests;

public class AuthenticateTests
{
    private static readonly string ChallengeEndpoint = TestServerBuilder.TestHost + TestServerBuilder.ChallengeWithProvider;

    [Fact]
    public async Task RegularGetRequestToCallbackPathSkips()
    {
        // Arrange
        var settings = new TestSettings();

        var server = settings.CreateTestServer(handler: async context =>
        {
            await context.Response.WriteAsync("Hi from the callback path");
        });

        // Act
        var transaction = await server.SendAsync("/");

        // Assert
        Assert.Equal("Hi from the callback path", transaction.ResponseText);
    }

    [Fact]
    public async Task RegularPostRequestToCallbackPathSkips()
    {
        // Arrange
        var settings = new TestSettings();

        var server = settings.CreateTestServer(handler: async context =>
        {
            await context.Response.WriteAsync("Hi from the callback path");
        });

        // Act
        var request = new HttpRequestMessage(HttpMethod.Post, "/");
        request.Content = new FormUrlEncodedContent(new Dictionary<string, string>());

        var transaction = await server.SendAsync(request, cookieHeader: null);

        // Assert
        Assert.Equal("Hi from the callback path", transaction.ResponseText);
    }

    [Fact]
    public async Task RegularGetRequestToCallbackPath()
    {
        var settings = new TestSettings();

        var server = settings.CreateTestServer();
        var transaction = await server.SendAsync(ChallengeEndpoint);

        var res = transaction.Response;
        Assert.Equal(HttpStatusCode.Redirect, res.StatusCode);
        Assert.NotNull(res.Headers.Location);

        var location = res.Headers.Location;
        var cookies = SetCookieHeaderValue.ParseList(transaction.SetCookie);

        var queryString = QueryHelpers.ParseQuery(location.OriginalString);

        var response = await GetAsync(server, $"{TestServerBuilder.TestHost}{SpidCieConst.CallbackPath}?state={queryString["state"]}&iss={UrlEncoder.Default.Encode("http://127.0.0.1:8000/oidc/op/")}&code=test_code", cookies);
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.NotNull(res.Headers.Location);
    }

    [Fact]
    public async Task ErrorOnGetRequestToCallbackPath()
    {
        var settings = new TestSettings();

        var server = settings.CreateTestServer();
        var transaction = await server.SendAsync(ChallengeEndpoint);

        var res = transaction.Response;
        Assert.Equal(HttpStatusCode.Redirect, res.StatusCode);
        Assert.NotNull(res.Headers.Location);

        var location = res.Headers.Location;
        var cookies = SetCookieHeaderValue.ParseList(transaction.SetCookie);

        var queryString = QueryHelpers.ParseQuery(location.OriginalString);

        await Assert.ThrowsAnyAsync<Exception>(async () => await GetAsync(server, $"{SpidCieConst.CallbackPath}?state={queryString["state"]}&error=test_error&error_description=error_description", cookies));
    }

    private Task<HttpResponseMessage> GetAsync(TestServer server, string path, IEnumerable<SetCookieHeaderValue> cookies)
    {
        var client = server.CreateClient();
        foreach (var cookie in cookies)
        {
            client.DefaultRequestHeaders.Add("Cookie", new CookieHeaderValue(cookie.Name, cookie.Value).ToString());
        }
        return client.GetAsync(path);
    }
}
