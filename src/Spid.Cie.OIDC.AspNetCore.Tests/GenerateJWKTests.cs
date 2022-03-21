using Microsoft.AspNetCore.Http;
using Spid.Cie.OIDC.AspNetCore.Models;
using Spid.Cie.OIDC.AspNetCore.OpenIdFederation;
using Spid.Cie.OIDC.AspNetCore.Tests.Mocks;
using System.Threading.Tasks;
using Xunit;

namespace Spid.Cie.OIDC.AspNetCore.Tests;

public class GenerateJWKTests
{
    [Fact]
    public async Task TestGenerateJWKTestsNext()
    {
        RequestDelegate next = (HttpContext hc) => Task.CompletedTask;
        HttpContext ctx = new DefaultHttpContext();
        var middleware = new JWKGeneratorMiddleware(next);
        await middleware.Invoke(ctx, new MockCryptoService());
    }

    [Fact]
    public async Task TestGenerateJWKTests()
    {
        RequestDelegate next = (HttpContext hc) => Task.CompletedTask;
        HttpContext ctx = new DefaultHttpContext();
        ctx.Request.Path = $"/{SpidCieConst.JWKGeneratorPath}";
        var middleware = new JWKGeneratorMiddleware(next);
        await middleware.Invoke(ctx, new MockCryptoService());
    }
}
