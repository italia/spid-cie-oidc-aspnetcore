using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Spid.Cie.OIDC.AspNetCore.Helpers;
using Spid.Cie.OIDC.AspNetCore.Models;
using Spid.Cie.OIDC.AspNetCore.Services;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Spid.Cie.OIDC.AspNetCore.Configuration;

internal class OpenIdConnectOptionsProvider : IOptionsMonitor<OpenIdConnectOptions>
{
    private readonly ConcurrentDictionary<string, Lazy<OpenIdConnectOptions>> _cache;
    private readonly IOptionsFactory<OpenIdConnectOptions> _optionsFactory;
    private readonly IIdentityProviderSelector _idpSelector;
    private readonly IConfigurationManager<OpenIdConnectConfiguration> _configurationManager;
    private readonly CustomHttpClientHandler _httpClientHandler;
    private readonly IHttpClientFactory _httpClientFactory;

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

    public OpenIdConnectOptions CurrentValue => Get(Options.DefaultName);

    public OpenIdConnectOptions Get(string name)
    {
        var provider = Task.Run(async () => await _idpSelector.GetSelectedIdentityProvider()).Result;
        var options = _cache.GetOrAdd(name, _ => new Lazy<OpenIdConnectOptions>(() => _optionsFactory.Create(name))).Value;
        if (name.Equals(SpidCieConst.AuthenticationScheme))
        {
            options.ConfigurationManager = _configurationManager;
            options.BackchannelHttpHandler = _httpClientHandler;
            options.Backchannel = _httpClientFactory.CreateClient("SpidCieBackchannel");
        }
        return options;
    }

    public IDisposable OnChange(Action<OpenIdConnectOptions, string> listener) => throw new NotImplementedException();
}


internal class CustomHttpClientHandler : HttpClientHandler
{
    private readonly IRelyingPartySelector _rpSelector;
    public CustomHttpClientHandler(IRelyingPartySelector rpSelector)
    {
        _rpSelector = rpSelector;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = await base.SendAsync(request, cancellationToken);
        if (response.Content.Headers.ContentType?.MediaType == "application/jose")
        {
            var provider = await _rpSelector.GetSelectedRelyingParty();
            if (provider is not null)
            {
                var token = await response.Content.ReadAsStringAsync();
                var key = provider.OpenIdCoreJWKs?.Keys?.FirstOrDefault();
                if (key is not null)
                {
                    (_, RSA privateKey) = key.GetRSAKeys();

                    var decodedToken = token.DecodeJose(privateKey).DecodeJWT();

                    var httpResponse = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
                    httpResponse.Content = new StringContent(decodedToken, Encoding.UTF8, "application/json");
                    return httpResponse;
                }
            }
        }
        return response;
    }
}