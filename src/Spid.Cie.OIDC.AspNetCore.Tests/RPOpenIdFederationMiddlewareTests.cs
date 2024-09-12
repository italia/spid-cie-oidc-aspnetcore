using Microsoft.AspNetCore.Http;
using Spid.Cie.OIDC.AspNetCore.Middlewares;
using Spid.Cie.OIDC.AspNetCore.Services;
using Spid.Cie.OIDC.AspNetCore.Services.Defaults;
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
        await middleware.Invoke(ctx, new RelyingPartiesHandler(new MockOptionsMonitorSpidCieOptions(), new DefaultRelyingPartiesRetriever()), new MockAggregatorsHandler(), new MockCryptoService());
    }

    [Fact]
    public async Task TestRPOpenIdFederationMiddleware()
    {
        RequestDelegate next = (HttpContext hc) => Task.CompletedTask;
        HttpContext ctx = new DefaultHttpContext();
        ctx.Request.Scheme = "http";
        ctx.Request.Host = new HostString("127.0.0.1:5000");
        ctx.Request.Path = $"/{SpidCieConst.EntityConfigurationPath}";
        var middleware = new RPOpenIdFederationMiddleware(next);
        await middleware.Invoke(ctx, new RelyingPartiesHandler(new MockOptionsMonitorSpidCieOptions(), new DefaultRelyingPartiesRetriever()), new MockAggregatorsHandler(), new MockCryptoService());
    }

    [Fact]
    public async Task TestRPOpenIdFederationMiddlewareEmpty()
    {
        RequestDelegate next = (HttpContext hc) => Task.CompletedTask;
        HttpContext ctx = new DefaultHttpContext();
        ctx.Request.Scheme = "http";
        ctx.Request.Host = new HostString("127.0.0.1:5000");
        ctx.Request.Path = $"/{SpidCieConst.EntityConfigurationPath}";
        var middleware = new RPOpenIdFederationMiddleware(next);
        await middleware.Invoke(ctx, new RelyingPartiesHandler(new MockOptionsMonitorSpidCieOptions(true), new DefaultRelyingPartiesRetriever()), new MockAggregatorsHandler(), new MockCryptoService());
    }

    [Fact]
    public async Task TestRPOpenIdFederationMiddlewareNoKeys()
    {
        RequestDelegate next = (HttpContext hc) => Task.CompletedTask;
        HttpContext ctx = new DefaultHttpContext();
        ctx.Request.Scheme = "http";
        ctx.Request.Host = new HostString("127.0.0.1:5000");
        ctx.Request.Path = $"/{SpidCieConst.EntityConfigurationPath}";
        var middleware = new RPOpenIdFederationMiddleware(next);
        await middleware.Invoke(ctx, new RelyingPartiesHandler(new MockOptionsMonitorSpidCieOptions(false, true), new DefaultRelyingPartiesRetriever()), new MockAggregatorsHandler(), new MockCryptoService());
    }

    [Fact]
    public async Task TestRPOpenIdFederationMiddlewareJson()
    {
        RequestDelegate next = (HttpContext hc) => Task.CompletedTask;
        HttpContext ctx = new DefaultHttpContext();
        ctx.Request.Scheme = "http";
        ctx.Request.Host = new HostString("127.0.0.1:5000");
        ctx.Request.Path = $"/{SpidCieConst.JsonEntityConfigurationPath}";
        var middleware = new RPOpenIdFederationMiddleware(next);
        await middleware.Invoke(ctx, new RelyingPartiesHandler(new MockOptionsMonitorSpidCieOptions(), new DefaultRelyingPartiesRetriever()), new MockAggregatorsHandler(), new MockCryptoService());
    }

}
