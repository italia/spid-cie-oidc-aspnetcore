using Microsoft.AspNetCore.Http;
using Spid.Cie.OIDC.AspNetCore.Middlewares;
using Spid.Cie.OIDC.AspNetCore.Models;
using Spid.Cie.OIDC.AspNetCore.Tests.Mocks;
using System.Threading.Tasks;
using Xunit;

namespace Spid.Cie.OIDC.AspNetCore.Tests;

public class RPOpenIdFederationMiddlewareTests
{
    [Fact]
    public async Task TestRPOpenIdFederationMiddlewareNext()
    {
        RequestDelegate next = (HttpContext hc) => Task.CompletedTask;
        HttpContext ctx = new DefaultHttpContext();
        var middleware = new RPOpenIdFederationMiddleware(next);
        await middleware.Invoke(ctx, new MockRelyingPartySelector(), new MockCryptoService());
    }

    [Fact]
    public async Task TestRPOpenIdFederationMiddleware()
    {
        RequestDelegate next = (HttpContext hc) => Task.CompletedTask;
        HttpContext ctx = new DefaultHttpContext();
        ctx.Request.Path = $"/{SpidCieConst.EntityConfigurationPath}";
        var middleware = new RPOpenIdFederationMiddleware(next);
        await middleware.Invoke(ctx, new MockRelyingPartySelector(), new MockCryptoService());
    }

    [Fact]
    public async Task TestRPOpenIdFederationMiddlewareEmpty()
    {
        RequestDelegate next = (HttpContext hc) => Task.CompletedTask;
        HttpContext ctx = new DefaultHttpContext();
        ctx.Request.Path = $"/{SpidCieConst.EntityConfigurationPath}";
        var middleware = new RPOpenIdFederationMiddleware(next);
        await middleware.Invoke(ctx, new MockRelyingPartySelector(true), new MockCryptoService());
    }

    [Fact]
    public async Task TestRPOpenIdFederationMiddlewareNoKeys()
    {
        RequestDelegate next = (HttpContext hc) => Task.CompletedTask;
        HttpContext ctx = new DefaultHttpContext();
        ctx.Request.Path = $"/{SpidCieConst.EntityConfigurationPath}";
        var middleware = new RPOpenIdFederationMiddleware(next);
        await middleware.Invoke(ctx, new MockRelyingPartySelector(false, true), new MockCryptoService());
    }
}
