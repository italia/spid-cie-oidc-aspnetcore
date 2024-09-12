using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.Net.Http.Headers;
using System;
using System.Linq;
using System.Net;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Xunit;

namespace Spid.Cie.OIDC.AspNetCore.Tests.IntegrationTests;

public class ChallengeTests
{
    private static readonly string ChallengeEndpoint = TestServerBuilder.TestHost + TestServerBuilder.ChallengeWithProvider;

    [Fact]
    public async Task ChallengeRedirectIsIssuedCorrectly()
    {
        var settings = new TestSettings();

        var server = settings.CreateTestServer();
        var transaction = await server.SendAsync(ChallengeEndpoint);

        var res = transaction.Response;
        Assert.Equal(HttpStatusCode.Redirect, res.StatusCode);
        Assert.NotNull(res.Headers.Location);

        settings.ValidateChallengeRedirect(
            res.Headers.Location,
            OpenIdConnectParameterNames.ClientId,
            OpenIdConnectParameterNames.ResponseType,
            OpenIdConnectParameterNames.Scope,
            OpenIdConnectParameterNames.RedirectUri,
            OpenIdConnectParameterNames.SkuTelemetry,
            OpenIdConnectParameterNames.VersionTelemetry);
    }

    [Fact]
    public async Task ChallengeIncludesPkceIfRequested()
    {
        var settings = new TestSettings();

        var server = settings.CreateTestServer();
        var transaction = await server.SendAsync(ChallengeEndpoint);

        var res = transaction.Response;
        Assert.Equal(HttpStatusCode.Redirect, res.StatusCode);
        Assert.NotNull(res.Headers.Location);

        Assert.Contains("code_challenge=", res.Headers.Location.Query);
        Assert.Contains("code_challenge_method=S256", res.Headers.Location.Query);
    }


    [Fact]
    public async Task ChallengeSetsNonceAndStateCookies()
    {
        var settings = new TestSettings();
        var server = settings.CreateTestServer();
        var transaction = await server.SendAsync(ChallengeEndpoint);

        Assert.Contains("samesite=none", transaction.SetCookie.First());
        var challengeCookies = SetCookieHeaderValue.ParseList(transaction.SetCookie);
        var nonceCookie = challengeCookies.Where(cookie => cookie.Name.StartsWith(OpenIdConnectDefaults.CookieNoncePrefix, StringComparison.Ordinal)).Single();
        Assert.True(nonceCookie.Expires.HasValue);
        Assert.True(nonceCookie.Expires > DateTime.UtcNow);
        Assert.True(nonceCookie.HttpOnly);
        Assert.Equal($"{SpidCieConst.CallbackPath}", nonceCookie.Path);
        Assert.Equal("N", nonceCookie.Value);
        Assert.Equal(SameSiteMode.None, nonceCookie.SameSite);

        var correlationCookie = challengeCookies.Where(cookie => cookie.Name.StartsWith(".AspNetCore.Correlation.", StringComparison.Ordinal)).Single();
        Assert.True(correlationCookie.Expires.HasValue);
        Assert.True(nonceCookie.Expires > DateTime.UtcNow);
        Assert.True(correlationCookie.HttpOnly);
        Assert.Equal($"{SpidCieConst.CallbackPath}", correlationCookie.Path);
        Assert.False(StringSegment.IsNullOrEmpty(correlationCookie.Value));
        Assert.Equal(SameSiteMode.None, correlationCookie.SameSite);

        Assert.Equal(2, challengeCookies.Count);
    }

    [Fact]
    public async Task Challenge_HasExpectedPromptParam()
    {
        var settings = new TestSettings();

        var server = settings.CreateTestServer();
        var transaction = await server.SendAsync(ChallengeEndpoint);

        var res = transaction.Response;

        Assert.Equal(HttpStatusCode.Redirect, res.StatusCode);
        settings.ValidateChallengeRedirect(res.Headers.Location, OpenIdConnectParameterNames.Prompt);
        Assert.Contains($"prompt={UrlEncoder.Default.Encode(SpidCieConst.Prompt)}", res.Headers.Location.Query);
    }
}
