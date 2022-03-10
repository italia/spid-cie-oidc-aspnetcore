using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;
using Spid.Cie.OIDC.AspNetCore.Models;
using Spid.Cie.OIDC.AspNetCore.Services;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Spid.Cie.OIDC.AspNetCore.Configuration;

internal class OpenIdConnectOptionsInitializer : IConfigureNamedOptions<OpenIdConnectOptions>
{
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly IIdentityProviderSelector _idpSelector;

    public OpenIdConnectOptionsInitializer(
        IDataProtectionProvider dataProtectionProvider,
        IIdentityProviderSelector idpSelector)
    {
        _dataProtectionProvider = dataProtectionProvider;
        _idpSelector = idpSelector;
    }

    public void Configure(string name, OpenIdConnectOptions options)
    {
        if (!name.StartsWith(SpidCieDefaults.AuthenticationScheme))
        {
            return;
        }

        var provider = Task.Run(async () => await _idpSelector.GetSelectedIdentityProvider()).Result;

        // Create a tenant-specific data protection provider to ensure
        // encrypted states can't be read/decrypted by the other tenants.
        options.DataProtectionProvider = _dataProtectionProvider.CreateProtector(provider.Name);

        // Other tenant-specific options like options.MetadataAddress can be registered here.
        options.MetadataAddress = provider.MetadataAddress;
    }

    public void Configure(OpenIdConnectOptions options)
        => Debug.Fail("This infrastructure method shouldn't be called.");
}
