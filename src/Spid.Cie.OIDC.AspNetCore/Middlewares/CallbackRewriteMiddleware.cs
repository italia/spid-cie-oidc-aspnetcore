using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System;
using System.Threading.Tasks;

namespace Spid.Cie.OIDC.AspNetCore.Middlewares;

class CallbackRewriteMiddleware
{
    readonly RequestDelegate _next;

    public CallbackRewriteMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        CheckAndReplaceCallbackPath(context, SpidCieConst.CallbackPath);
        CheckAndReplaceCallbackPath(context, SpidCieConst.SignedOutCallbackPath);
        CheckAndReplaceCallbackPath(context, SpidCieConst.RemoteSignOutPath);

        await _next(context);

        return;
    }

    private static void CheckAndReplaceCallbackPath(HttpContext context, string tail)
    {
        if (context.Request.Path.Value!.EndsWith(tail, StringComparison.OrdinalIgnoreCase)
                && !context.Request.Path.Value!.Equals(tail, StringComparison.OrdinalIgnoreCase))
        {
            context.Request.Headers.Add("X-Replaced-Path", new StringValues(context.Request.Path));
            context.Request.Path = tail;
        }
    }
}