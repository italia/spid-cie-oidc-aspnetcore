using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Security.Claims;

namespace Spid.Cie.OIDC.AspNetCore.Tests.Mocks;

internal class MockHttpContextAccessor : IHttpContextAccessor
{
    private readonly bool _hasQSValue;
    private readonly bool _addClaims;

    public MockHttpContextAccessor(bool hasQSValue, bool addClaims = true)
    {
        _hasQSValue = hasQSValue;
        _addClaims = addClaims;
    }

    public HttpContext? HttpContext
    {
        get
        {
            var context = new DefaultHttpContext();
            context.Request.Host = new HostString("127.0.0.1", 5000);
            context.Request.Scheme = "http";

            if (_addClaims)
                context.User = new System.Security.Claims.ClaimsPrincipal(new ClaimsIdentity(new List<Claim>() {
                    new Claim("iss", "http://127.0.0.1:8000/oidc/op/"),
                    new Claim("aud", "http://127.0.0.1:5000/"),
                }));
            if (_hasQSValue)
                context.Request.QueryString = context.Request.QueryString.Add(SpidCieConst.IdPSelectorKey, "http://127.0.0.1:8000/oidc/op/");
            return context;
        }
        set => throw new System.NotImplementedException();
    }
}
