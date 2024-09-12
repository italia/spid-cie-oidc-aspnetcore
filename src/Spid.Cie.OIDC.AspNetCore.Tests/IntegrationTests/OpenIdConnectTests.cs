using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using System;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace Spid.Cie.OIDC.AspNetCore.Tests.IntegrationTests;

public class OpenIdConnectTests
{
    static readonly string noncePrefix = "OpenIdConnect." + "Nonce.";
    static readonly string nonceDelimiter = ".";
    const string DefaultHost = @"http://127.0.0.1:5000";

    /// <summary>
    /// Tests RedirectForSignOutContext replaces the OpenIdConnectMesssage correctly.
    /// </summary>
    /// <returns>Task</returns>
    [Fact]
    public async Task SignOutSettingMessage()
    {
        var setting = new TestSettings();

        var server = setting.CreateTestServer();

        var transaction = await server.SendAsync(DefaultHost + TestServerBuilder.Signout);
        var res = transaction.Response;

        Assert.Equal(HttpStatusCode.Redirect, res.StatusCode);
        Assert.NotNull(res.Headers.Location);
        Assert.NotNull("/");
    }

    [Fact]
    public async Task RedirectToIdentityProvider_SetsNonceCookiePath_ToCallBackPath()
    {
        var setting = new TestSettings();

        var server = setting.CreateTestServer();

        var transaction = await server.SendAsync(DefaultHost + TestServerBuilder.ChallengeWithProvider);
        var res = transaction.Response;

        Assert.Equal(HttpStatusCode.Redirect, res.StatusCode);
        Assert.NotNull(res.Headers.Location);
        var setCookie = Assert.Single(res.Headers, h => h.Key == "Set-Cookie");
        var nonce = Assert.Single(setCookie.Value, v => v.StartsWith(OpenIdConnectDefaults.CookieNoncePrefix, StringComparison.Ordinal));
        Assert.Contains($"path={SpidCieConst.CallbackPath}", nonce);
    }

    [Fact]
    public async Task RedirectToIdentityProvider_SetsCorrelationIdCookiePath_ToCallBackPath()
    {
        var setting = new TestSettings();

        var server = setting.CreateTestServer();

        var transaction = await server.SendAsync(DefaultHost + TestServerBuilder.ChallengeWithProvider);
        var res = transaction.Response;

        Assert.Equal(HttpStatusCode.Redirect, res.StatusCode);
        Assert.NotNull(res.Headers.Location);
        var setCookie = Assert.Single(res.Headers, h => h.Key == "Set-Cookie");
        var correlation = Assert.Single(setCookie.Value, v => v.StartsWith(".AspNetCore.Correlation.", StringComparison.Ordinal));
        Assert.Contains($"path={SpidCieConst.CallbackPath}", correlation);
    }

    [Fact]
    public async Task RemoteSignOut_Get_Successful()
    {
        var settings = new TestSettings();
        var server = settings.CreateTestServer(handler: async context =>
        {
            var claimsIdentity = new ClaimsIdentity("Cookies");
            claimsIdentity.AddClaim(new Claim("iss", "test"));
            claimsIdentity.AddClaim(new Claim("sid", "something"));
            await context.SignInAsync(new ClaimsPrincipal(claimsIdentity));
        });

        var signInTransaction = await server.SendAsync(DefaultHost);

        var remoteSignOutTransaction = await server.SendAsync(DefaultHost + "/signout-spidcie?iss=test&sid=something", signInTransaction.AuthenticationCookieValue);
        Assert.Equal(HttpStatusCode.OK, remoteSignOutTransaction.Response.StatusCode);
        Assert.Contains(remoteSignOutTransaction.Response.Headers, h => h.Key == "Set-Cookie");
    }

}
