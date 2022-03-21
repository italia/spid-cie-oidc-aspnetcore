using Microsoft.AspNetCore.Http;
using Spid.Cie.OIDC.AspNetCore.Models;
using System.Collections.Generic;
using System.Security.Claims;

namespace Spid.Cie.OIDC.AspNetCore.Tests.Mocks;

internal class MockHttpContextAccessor : IHttpContextAccessor
{
    private readonly bool _hasQSValue;

    public MockHttpContextAccessor(bool hasQSValue)
    {
        _hasQSValue = hasQSValue;
    }

    public HttpContext? HttpContext
    {
        get
        {
            var context = new DefaultHttpContext();
            context.User = new System.Security.Claims.ClaimsPrincipal(new ClaimsIdentity(new List<Claim>() {
                new Claim("iss", "http://127.0.0.1:8000/"),
                new Claim("aud", "http://127.0.0.1:5000/"),
            }));
            if (_hasQSValue)
                context.Request.QueryString = context.Request.QueryString.Add(SpidCieConst.IdPSelectorKey, "http://127.0.0.1:8000/");
            return context;
        }
        set => throw new System.NotImplementedException();
    }
}
