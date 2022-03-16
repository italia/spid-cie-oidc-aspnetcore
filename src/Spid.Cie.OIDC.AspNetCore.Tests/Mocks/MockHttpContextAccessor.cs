using Microsoft.AspNetCore.Http;
using Spid.Cie.OIDC.AspNetCore.Models;

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
            if (_hasQSValue)
                context.Request.QueryString = context.Request.QueryString.Add(SpidCieConst.IdPSelectorKey, "MockIdP");
            return context;
        }
        set => throw new System.NotImplementedException();
    }
}
