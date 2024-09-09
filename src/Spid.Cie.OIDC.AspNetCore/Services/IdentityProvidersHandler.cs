using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Spid.Cie.OIDC.AspNetCore.Configuration;
using Spid.Cie.OIDC.AspNetCore.Enums;
using Spid.Cie.OIDC.AspNetCore.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Spid.Cie.OIDC.AspNetCore.Services;

class IdentityProvidersHandler : IIdentityProvidersHandler
{
    readonly ITrustChainManager _trustChainManager;
    readonly IOptionsMonitor<SpidCieOptions> _options;
    readonly IIdentityProvidersRetriever _idpRetriever;
    readonly ILogger<IdentityProvidersHandler> _logger;

    public IdentityProvidersHandler(IOptionsMonitor<SpidCieOptions> options, IIdentityProvidersRetriever idpRetriever,
                                    ITrustChainManager trustChainManager, ILogger<IdentityProvidersHandler> logger)
    {
        _logger = logger;
        _options = options;
        _idpRetriever = idpRetriever;
        _trustChainManager = trustChainManager;
    }

    public async Task<IEnumerable<IdentityProvider>> GetIdentityProviders()
    {
        List<IdentityProvider?> result = new();

        var idpUrls = _options.CurrentValue.CieOPs.Union(await _idpRetriever.GetCieIdentityProviders()).Select(ip => new
        {
            Type = IdentityProviderTypes.CIE,
            Url = ip
        }).Union(_options.CurrentValue.SpidOPs.Union(await _idpRetriever.GetSpidIdentityProviders()).Select(ip => new
        {
            Type = IdentityProviderTypes.SPID,
            Url = ip
        })).ToList();

        foreach (var idp in idpUrls)
        {
            var idpConf = await _trustChainManager.BuildTrustChain(idp.Url);

            if (idpConf != null)
                result.Add(idp.Type == IdentityProviderTypes.CIE ? CreateIdentityProvider<CieIdentityProvider>(idpConf) :
                            CreateIdentityProvider<SpidIdentityProvider>(idpConf));
        }

        return result.Where(r => r != default).ToList()!;
    }

    static T? CreateIdentityProvider<T>(OPEntityConfiguration conf)
        where T : IdentityProvider
    {
        return conf == default ? default :
            typeof(T).Equals(typeof(SpidIdentityProvider)) ?
            new SpidIdentityProvider()
            {
                EntityConfiguration = conf,
                Uri = conf.Subject ?? string.Empty,
                OrganizationLogoUrl = conf.Metadata.OpenIdProvider.AdditionalData.TryGetValue("logo_uri", out object? spidLogoUri) ? spidLogoUri as string ?? string.Empty : string.Empty,
                OrganizationName = conf.Metadata.OpenIdProvider.AdditionalData.TryGetValue("organization_name", out object? spidOrganizationName) ? spidOrganizationName as string ?? string.Empty : string.Empty,
                SupportedAcrValues = conf.Metadata.OpenIdProvider.AcrValuesSupported.ToList(),
            } as T :
            typeof(T).Equals(typeof(CieIdentityProvider)) ?
            new CieIdentityProvider()
            {
                EntityConfiguration = conf,
                Uri = conf.Subject ?? string.Empty,
                OrganizationLogoUrl = conf.Metadata.OpenIdProvider.AdditionalData.TryGetValue("logo_uri", out object? cieLogoUri) ? cieLogoUri as string ?? string.Empty : string.Empty,
                OrganizationName = conf.Metadata.OpenIdProvider.AdditionalData.TryGetValue("organization_name", out object? cieOrganizationName) ? cieOrganizationName as string ?? string.Empty : string.Empty,
                SupportedAcrValues = conf.Metadata.OpenIdProvider.AcrValuesSupported.ToList(),
            } as T : default;
    }
}