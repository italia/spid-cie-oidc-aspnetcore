using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Options;
using Spid.Cie.OIDC.AspNetCore.Services;
using static Spid.Cie.OIDC.AspNetCore.Tests.IntegrationTests.TestSettings;

namespace Spid.Cie.OIDC.AspNetCore.Tests.Mocks
{
    internal class MockOpenIdConnectPostConfigureOptions : IPostConfigureOptions<OpenIdConnectOptions>
    {
        public void PostConfigure(string name, OpenIdConnectOptions options)
        {
            options.ProtocolValidator = new TestProtocolValidator();
            options.TokenValidationParameters.ValidateLifetime = false;
            options.SecurityTokenValidator = new MockSecurityTokenHandler();
        }
    }
}
