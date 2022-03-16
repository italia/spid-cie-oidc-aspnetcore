using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using Spid.Cie.OIDC.AspNetCore.Configuration;
using Spid.Cie.OIDC.AspNetCore.Helpers;
using Spid.Cie.OIDC.AspNetCore.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Spid.Cie.OIDC.AspNetCore.Services;

internal class IdentityProvidersRetriever : IIdentityProvidersRetriever
{
    private readonly HttpClient _client;
    private readonly IOptionsMonitor<SpidCieOptions> _options;

    public IdentityProvidersRetriever(IOptionsMonitor<SpidCieOptions> options, HttpClient client)
    {
        _client = client;
        _options = options;
    }

    public async Task<IEnumerable<IdentityProvider>> GetIdentityProviders()
    {
        List<IdentityProvider> result = new();

        var urlsString = await _client.GetStringAsync($"{_options.CurrentValue.TrustAnchorUrl.EnsureTrailingSlash()}{SpidCieConst.OPListPath}");
        var urls = JsonSerializer.Deserialize<IEnumerable<string>>(urlsString);
        foreach (var url in urls ?? Enumerable.Empty<string>())
        {
            var metadataAddress = $"{url.EnsureTrailingSlash()}{SpidCieConst.EntityConfigurationPath}";
            var jwt = await _client.GetStringAsync(metadataAddress);
            if (!string.IsNullOrWhiteSpace(jwt))
            {
                var decodedJwt = jwt.DecodeJWT();
                var conf = JsonSerializer.Deserialize<IdPEntityConfiguration>(decodedJwt);
                if (conf != null)
                {
                    conf.Metadata.OpenIdProvider = OpenIdConnectConfiguration.Create(JObject.Parse(decodedJwt)["metadata"]["openid_provider"].ToString());
                    conf.Metadata.OpenIdProvider.JsonWebKeySet = JsonWebKeySet.Create(JObject.Parse(decodedJwt)["metadata"]["openid_provider"]["jwks"].ToString());
                    result.Add(new SpidIdentityProvider()
                    {
                        EntityConfiguration = conf,
                        Uri = conf.Metadata.OpenIdProvider.AdditionalData["op_uri"] as string ?? string.Empty,
                        OrganizationDisplayName = conf.Metadata.OpenIdProvider.AdditionalData["op_name"] as string ?? string.Empty,
                        OrganizationUrl = conf.Metadata.OpenIdProvider.AdditionalData["op_uri"] as string ?? string.Empty,
                        OrganizationLogoUrl = conf.Metadata.OpenIdProvider.AdditionalData["logo_uri"] as string ?? string.Empty,
                        OrganizationName = conf.Metadata.OpenIdProvider.AdditionalData["organization_name"] as string ?? string.Empty,
                        SupportedAcrValues = conf.Metadata.OpenIdProvider.AcrValuesSupported.ToArray(),
                    });
                }
            }
        }

        return result;
    }
}
