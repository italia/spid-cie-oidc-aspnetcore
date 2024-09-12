using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Spid.Cie.OIDC.AspNetCore.Services;
using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading.Tasks;

namespace Spid.Cie.OIDC.AspNetCore.Configuration;

class OpenIdConnectOptionsProvider : IOptionsMonitor<OpenIdConnectOptions>
{
    readonly ConcurrentDictionary<string, Lazy<OpenIdConnectOptions>> _cache;
    readonly IOptionsFactory<OpenIdConnectOptions> _optionsFactory;
    readonly IIdentityProviderSelector _idpSelector;
    readonly IConfigurationManager<OpenIdConnectConfiguration> _configurationManager;
    readonly CustomHttpClientHandler _httpClientHandler;
    readonly IHttpClientFactory _httpClientFactory;

    public OpenIdConnectOptionsProvider(
        IOptionsFactory<OpenIdConnectOptions> optionsFactory,
        IIdentityProviderSelector idpSelector,
        IConfigurationManager<OpenIdConnectConfiguration> configurationManager,
        CustomHttpClientHandler httpClientHandler,
        IHttpClientFactory httpClientFactory)
    {
        _cache = new ConcurrentDictionary<string, Lazy<OpenIdConnectOptions>>();
        _optionsFactory = optionsFactory;
        _idpSelector = idpSelector;
        _configurationManager = configurationManager;
        _httpClientHandler = httpClientHandler;
        _httpClientFactory = httpClientFactory;
    }

    [ExcludeFromCodeCoverage]
    public OpenIdConnectOptions CurrentValue => Get(Options.DefaultName);

    public OpenIdConnectOptions Get(string name)
    {
        var provider = Task.Run(async () => await _idpSelector.GetSelectedIdentityProvider()).Result;
        var options = _cache.GetOrAdd(name, _ => new Lazy<OpenIdConnectOptions>(() => _optionsFactory.Create(name))).Value;
        if (name.Equals(SpidCieConst.AuthenticationScheme))
        {
            options.ConfigurationManager = _configurationManager;
            options.BackchannelHttpHandler = _httpClientHandler;
            options.Backchannel = _httpClientFactory.CreateClient(SpidCieConst.BackchannelClientName);
        }
        return options;
    }

    [ExcludeFromCodeCoverage]
    public IDisposable OnChange(Action<OpenIdConnectOptions, string> listener) => throw new NotImplementedException();
}