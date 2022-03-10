using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Options;
using Spid.Cie.OIDC.AspNetCore.Services;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Spid.Cie.OIDC.AspNetCore.Configuration;

internal class OpenIdConnectOptionsProvider : IOptionsMonitor<OpenIdConnectOptions>
{
    private readonly ConcurrentDictionary<string, Lazy<OpenIdConnectOptions>> _cache;
    private readonly IOptionsFactory<OpenIdConnectOptions> _optionsFactory;
    private readonly IIdentityProviderSelector _idpSelector;
    public OpenIdConnectOptionsProvider(
        IOptionsFactory<OpenIdConnectOptions> optionsFactory,
        IIdentityProviderSelector idpSelector)
    {
        _cache = new ConcurrentDictionary<string, Lazy<OpenIdConnectOptions>>();
        _optionsFactory = optionsFactory;
        _idpSelector = idpSelector;
    }

    public OpenIdConnectOptions CurrentValue => Get(Options.DefaultName);

    public OpenIdConnectOptions Get(string name)
    {
        var provider = Task.Run(async () => await _idpSelector.GetSelectedIdentityProvider()).Result;

        var fullName = $"{name}_{provider.Name}";

        return _cache.GetOrAdd(fullName, _ => new Lazy<OpenIdConnectOptions>(() => _optionsFactory.Create(fullName))).Value;
    }

    public IDisposable OnChange(Action<OpenIdConnectOptions, string> listener) => null;
}
