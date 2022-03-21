using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Threading;
using System.Threading.Tasks;

namespace Spid.Cie.OIDC.AspNetCore.Tests.Mocks
{
    internal class MockConfigurationManager : IConfigurationManager<OpenIdConnectConfiguration>
    {
        public async Task<OpenIdConnectConfiguration> GetConfigurationAsync(CancellationToken cancel)
        {
            await Task.CompletedTask;
            return new OpenIdConnectConfiguration();
        }

        public void RequestRefresh()
        {

        }
    }
}
