using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Spid.Cie.OIDC.AspNetCore.Configuration;
using Spid.Cie.OIDC.AspNetCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Spid.Cie.OIDC.AspNetCore.Services;

internal class IdentityProvidersHandler : IIdentityProvidersHandler
{
    private readonly IOptionsMonitor<SpidCieOptions> _options;
    private readonly IIdentityProvidersRetriever _idpRetriever;
    private readonly ITrustChainManager _trustChainManager;
    private readonly ILogger<IdentityProvidersHandler> _logger;
    private static readonly SemaphoreSlim _syncLock = new SemaphoreSlim(1);
    private static readonly List<IdentityProvider> _identityProvidersCache = new();
    private static DateTime _identityProvidersCacheLastUpdated = DateTime.MinValue;

    public IdentityProvidersHandler(IOptionsMonitor<SpidCieOptions> options,
        IIdentityProvidersRetriever idpRetriever,
        ITrustChainManager trustChainManager,
        ILogger<IdentityProvidersHandler> logger)
    {
        _options = options;
        _idpRetriever = idpRetriever;
        _trustChainManager = trustChainManager;
        _logger = logger;
    }

    public async Task<IEnumerable<IdentityProvider>> GetIdentityProviders()
    {
        if (_identityProvidersCacheLastUpdated.AddMinutes(_options.CurrentValue.IdentityProvidersCacheExpirationInMinutes) < DateTime.UtcNow)
        {
            if (!await _syncLock.WaitAsync(TimeSpan.FromSeconds(10)))
            {
                _logger.LogWarning("IdentityProvider Sync Lock expired.");
                return Enumerable.Empty<IdentityProvider>();
            }
            try
            {
                if (_identityProvidersCacheLastUpdated.AddMinutes(_options.CurrentValue.IdentityProvidersCacheExpirationInMinutes) < DateTime.UtcNow)
                {
                    _identityProvidersCache.Clear();

                    List<IdentityProvider> result = new();

                    var spidIdP = _options.CurrentValue.SpidOPs
                        .Union(await _idpRetriever.GetSpidIdentityProviders());
                    foreach (var url in spidIdP)
                    {
                        var idpConf = await _trustChainManager.BuildTrustChain(url);

                        if (idpConf is not null)
                        {
                            result.Add(CreateSpidIdentityProvider(idpConf));
                        }
                    }

                    var cieIdP = _options.CurrentValue.CieOPs
                        .Union(await _idpRetriever.GetCieIdentityProviders());
                    foreach (var url in cieIdP)
                    {
                        var idpConf = await _trustChainManager.BuildTrustChain(url);

                        if (idpConf is not null)
                        {
                            result.Add(CreateCieIdentityProvider(idpConf));
                        }
                    }

                    if (result.Count > 0)
                    {
                        _identityProvidersCache.AddRange(result);
                        _identityProvidersCacheLastUpdated = DateTime.UtcNow;
                    }
                }
            }
            finally
            {
                _syncLock.Release();
            }
        }
        return _identityProvidersCache;
    }

    private static IdentityProvider CreateSpidIdentityProvider(IdPEntityConfiguration conf)
       => new SpidIdentityProvider()
       {
           EntityConfiguration = conf,
           Uri = conf.Subject ?? string.Empty,
           OrganizationDisplayName = conf.Metadata.OpenIdProvider!.AdditionalData["op_name"] as string ?? string.Empty,
           OrganizationUrl = conf.Metadata.OpenIdProvider.AdditionalData["op_uri"] as string ?? string.Empty,
           OrganizationLogoUrl = conf.Metadata.OpenIdProvider.AdditionalData["logo_uri"] as string ?? string.Empty,
           OrganizationName = conf.Metadata.OpenIdProvider.AdditionalData["organization_name"] as string ?? string.Empty,
           SupportedAcrValues = conf.Metadata.OpenIdProvider.AcrValuesSupported.ToList(),
       };

    private static IdentityProvider CreateCieIdentityProvider(IdPEntityConfiguration conf)
       => new CieIdentityProvider()
       {
           EntityConfiguration = conf,
           Uri = conf.Subject ?? string.Empty,
           OrganizationDisplayName = conf.Metadata.OpenIdProvider!.AdditionalData["op_name"] as string ?? string.Empty,
           OrganizationUrl = conf.Metadata.OpenIdProvider.AdditionalData["op_uri"] as string ?? string.Empty,
           OrganizationLogoUrl = conf.Metadata.OpenIdProvider.AdditionalData["logo_uri"] as string ?? string.Empty,
           OrganizationName = conf.Metadata.OpenIdProvider.AdditionalData["organization_name"] as string ?? string.Empty,
           SupportedAcrValues = conf.Metadata.OpenIdProvider.AcrValuesSupported.ToList(),
       };
}
