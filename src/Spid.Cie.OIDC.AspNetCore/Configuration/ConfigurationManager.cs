using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Spid.Cie.OIDC.AspNetCore.Services;
using System.Threading;
using System.Threading.Tasks;

namespace Spid.Cie.OIDC.AspNetCore.Configuration
{
    internal class ConfigurationManager : IConfigurationManager<OpenIdConnectConfiguration>
    {
        private readonly IIdentityProviderSelector _idpSelector;

        public ConfigurationManager(IIdentityProviderSelector idpSelector)
        {
            _idpSelector = idpSelector;
        }

        public async Task<OpenIdConnectConfiguration> GetConfigurationAsync(CancellationToken cancel)
        {
            var idp = await _idpSelector.GetSelectedIdentityProvider();
            if (idp != null)
            {
                return (idp?.EntityConfiguration?.Metadata)?.OpenIdProvider;
            }
            return null;
        }

        public void RequestRefresh() { }
    }
}
