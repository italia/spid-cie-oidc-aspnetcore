using Microsoft.AspNetCore.Http;
using Spid.Cie.OIDC.AspNetCore.Middlewares;
using Spid.Cie.OIDC.AspNetCore.Services;
using Spid.Cie.OIDC.AspNetCore.Services.Defaults;
using Spid.Cie.OIDC.AspNetCore.Tests.Mocks;
using System.Threading.Tasks;
using Xunit;

namespace Spid.Cie.OIDC.AspNetCore.Tests;

public class FetchOpenIdFederationMiddlewareTests
{
    [Fact]
    public async Task TestFetchOpenIdFederationMiddlewareNext()
    {
        RequestDelegate next = (HttpContext hc) => Task.CompletedTask;
        HttpContext ctx = new DefaultHttpContext();
        var middleware = new FetchOpenIdFederationMiddleware(next);
        await middleware.Invoke(ctx, new RelyingPartiesHandler(new MockOptionsMonitorSpidCieOptions(), new DefaultRelyingPartiesRetriever()), new MockAggregatorsHandler(), new MockCryptoService());
    }

    [Fact]
    public async Task TestFetchOpenIdFederationMiddlewareNoIssNoSub()
    {
        RequestDelegate next = (HttpContext hc) => Task.CompletedTask;
        HttpContext ctx = new DefaultHttpContext();
        ctx.Request.Scheme = "http";
        ctx.Request.Host = new HostString("127.0.0.1:5000");
        ctx.Request.Path = $"/{SpidCieConst.FetchEndpointPath}";
        var middleware = new FetchOpenIdFederationMiddleware(next);
        await middleware.Invoke(ctx, new RelyingPartiesHandler(new MockOptionsMonitorSpidCieOptions(), new DefaultRelyingPartiesRetriever()), new MockAggregatorsHandler(), new MockCryptoService());
    }

    [Fact]
    public async Task TestFetchOpenIdFederationMiddlewareEmpty()
    {
        RequestDelegate next = (HttpContext hc) => Task.CompletedTask;
        HttpContext ctx = new DefaultHttpContext();
        ctx.Request.Scheme = "http";
        ctx.Request.Host = new HostString("127.0.0.1:5000");
        ctx.Request.Path = $"/{SpidCieConst.FetchEndpointPath}";
        ctx.Request.QueryString = new QueryString($"?iss=http://127.0.0.1:5000/&sub=http://127.0.0.1:5000/");

        var middleware = new FetchOpenIdFederationMiddleware(next);
        await middleware.Invoke(ctx, new RelyingPartiesHandler(new MockOptionsMonitorSpidCieOptions(true), new DefaultRelyingPartiesRetriever()), new MockAggregatorsHandler(), new MockCryptoService());
    }

    [Fact]
    public async Task TestFetchOpenIdFederationMiddlewareNoKeys()
    {
        RequestDelegate next = (HttpContext hc) => Task.CompletedTask;
        HttpContext ctx = new DefaultHttpContext();
        ctx.Request.Scheme = "http";
        ctx.Request.Host = new HostString("127.0.0.1:5000");
        ctx.Request.Path = $"/{SpidCieConst.FetchEndpointPath}";
        var middleware = new FetchOpenIdFederationMiddleware(next);
        await middleware.Invoke(ctx, new RelyingPartiesHandler(new MockOptionsMonitorSpidCieOptions(false, true), new DefaultRelyingPartiesRetriever()), new MockAggregatorsHandler(), new MockCryptoService());
    }

}
