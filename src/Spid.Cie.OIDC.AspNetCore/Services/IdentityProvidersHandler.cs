using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Spid.Cie.OIDC.AspNetCore.Configuration;
using Spid.Cie.OIDC.AspNetCore.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Spid.Cie.OIDC.AspNetCore.Services;

internal class IdentityProvidersHandler : IIdentityProvidersHandler
{
    private readonly IOptionsMonitor<SpidCieOptions> _options;
    private readonly IIdentityProvidersRetriever _idpRetriever;
    private readonly ITrustChainManager _trustChainManager;
    private readonly ILogger<IdentityProvidersHandler> _logger;

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
        List<IdentityProvider> result = new();

        var spidIdP = _options.CurrentValue.SpidOPs
            .Union(await _idpRetriever.GetSpidIdentityProviders())
            .ToList();
        foreach (var url in spidIdP)
        {
            var idpConf = await _trustChainManager.BuildTrustChain(url);

            if (idpConf is not null)
            {
                result.Add(CreateSpidIdentityProvider(idpConf));
            }
        }

        var cieIdP = _options.CurrentValue.CieOPs
            .Union(await _idpRetriever.GetCieIdentityProviders())
            .ToList();
        foreach (var url in cieIdP)
        {
            var idpConf = await _trustChainManager.BuildTrustChain(url);

            if (idpConf is not null)
            {
                result.Add(CreateCieIdentityProvider(idpConf));
            }
        }

        return result;
    }

	private static IdentityProvider CreateSpidIdentityProvider(IdPEntityConfiguration conf)
		=> new SpidIdentityProvider()
		{
			EntityConfiguration = conf,
			Uri = conf.Subject ?? string.Empty,
			OrganizationLogoUrl = conf.Metadata.OpenIdProvider.AdditionalData.TryGetValue("logo_uri", out object? logoUri) ? logoUri as string ?? string.Empty : string.Empty,
			OrganizationName = conf.Metadata.OpenIdProvider.AdditionalData.TryGetValue("organization_name", out object? organizationName) ? organizationName as string ?? string.Empty : string.Empty,
			SupportedAcrValues = conf.Metadata.OpenIdProvider.AcrValuesSupported.ToList(),
		};

	private static IdentityProvider CreateCieIdentityProvider(IdPEntityConfiguration conf)
		=> new CieIdentityProvider()
		{
			EntityConfiguration = conf,
			Uri = conf.Subject ?? string.Empty,
			OrganizationLogoUrl = conf.Metadata.OpenIdProvider.AdditionalData.TryGetValue("logo_uri", out object? logoUri) ? logoUri as string ?? string.Empty : string.Empty,
			OrganizationName = conf.Metadata.OpenIdProvider.AdditionalData.TryGetValue("organization_name", out object? organizationName) ? organizationName as string ?? string.Empty : string.Empty,
			SupportedAcrValues = conf.Metadata.OpenIdProvider.AcrValuesSupported.ToList(),
		};
}
