using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Options;
using Spid.Cie.OIDC.AspNetCore.Models;
using System;
using static Spid.Cie.OIDC.AspNetCore.Tests.IntegrationTests.TestSettings;

namespace Spid.Cie.OIDC.AspNetCore.Tests.Mocks;

internal class MockOptionsMonitorOpenIdConnectOptions : IOptionsMonitor<OpenIdConnectOptions>
{
    public OpenIdConnectOptions CurrentValue => Get(SpidCieConst.AuthenticationScheme);

    public OpenIdConnectOptions Get(string name)
    {
        return new OpenIdConnectOptions()
        {
            SignInScheme = name,
            ConfigurationManager = new MockConfigurationManager(),
            ProtocolValidator = new TestProtocolValidator()
        };
    }

    public IDisposable OnChange(Action<OpenIdConnectOptions, string> listener)
    {
        throw new NotImplementedException();
    }
}
