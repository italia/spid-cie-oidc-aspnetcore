using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Spid.Cie.OIDC.AspNetCore.Configuration;
using Spid.Cie.OIDC.AspNetCore.Helpers;
using Spid.Cie.OIDC.AspNetCore.Models;
using Spid.Cie.OIDC.AspNetCore.OpenIdFederation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Spid.Cie.OIDC.AspNetCore.Services;

internal class IdentityProvidersRetriever : IIdentityProvidersRetriever
{
    private readonly HttpClient _client;
    private readonly IOptionsMonitor<SpidCieOptions> _options;
    private readonly ITrustChainManager _trustChainManager;
    private readonly ILogger<IdentityProvidersRetriever> _logger;
    private static readonly SemaphoreSlim _syncLock = new SemaphoreSlim(1);
    private static List<IdentityProvider>? _identityProvidersCache;
    private static DateTime _identityProvidersCacheLastUpdated = DateTime.MinValue;

    public IdentityProvidersRetriever(IOptionsMonitor<SpidCieOptions> options,
        HttpClient client,
        ITrustChainManager trustChainManager,
        ILogger<IdentityProvidersRetriever> logger)
    {
        _client = client;
        _options = options;
        _trustChainManager = trustChainManager;
        _logger = logger;
    }

    public async Task<IEnumerable<IdentityProvider>> GetIdentityProviders()
    {
        if (_identityProvidersCache is null
            || _identityProvidersCacheLastUpdated.AddMinutes(_options.CurrentValue.IdentityProvidersCacheExpirationInMinutes) < DateTime.UtcNow)
        {
            if (!await _syncLock.WaitAsync(TimeSpan.FromSeconds(10)))
            {
                _logger.LogWarning("IdentityProvider Sync Lock expired.");
                return Enumerable.Empty<IdentityProvider>();
            }
            try
            {
                if (_identityProvidersCache is null
                    || _identityProvidersCacheLastUpdated.AddMinutes(_options.CurrentValue.IdentityProvidersCacheExpirationInMinutes) < DateTime.UtcNow)
                {
                    List<IdentityProvider> result = new();

                    var urlsString = await _client.GetStringAsync($"{_options.CurrentValue.TrustAnchorUrl.EnsureTrailingSlash()}{SpidCieConst.OPListPath}");
                    var urls = JsonSerializer.Deserialize<IEnumerable<string>>(urlsString);
                    foreach (var url in urls ?? Enumerable.Empty<string>())
                    {
                        var identityProvider = await _trustChainManager.BuildTrustChain(url);

                        if (identityProvider is not null)
                        {
                            result.Add(identityProvider);
                        }
                    }

                    if (result.Count > 0)
                    {
                        _identityProvidersCache = result;
                        _identityProvidersCacheLastUpdated = DateTime.UtcNow;
                    }
                }
            }
            finally
            {
                _syncLock.Release();
            }
        }
        return _identityProvidersCache ?? Enumerable.Empty<IdentityProvider>();
    }
}
