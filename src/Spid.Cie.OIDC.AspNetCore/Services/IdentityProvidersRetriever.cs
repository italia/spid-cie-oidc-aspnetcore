using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
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

    public IdentityProvidersRetriever(HttpClient client)
    {
        _client = client;
    }

    public async Task<IEnumerable<IdentityProvider>> GetIdentityProviders()
    {
        List<IdentityProvider> result = new();

        var urlsString = await _client.GetStringAsync("http://127.0.0.1:8000/list/?type=openid_provider");
        var urls = JsonSerializer.Deserialize<IEnumerable<string>>(urlsString);
        foreach (var url in urls)
        {
            var metadataAddress = $"{url.EnsureTrailingSlash()}{SpidCieDefaults.EntityConfigurationPath}";
            var jwt = await _client.GetStringAsync(metadataAddress);
            var decodedJwt = jwt.DecodeJWT();
            var conf = System.Text.Json.JsonSerializer.Deserialize<IdPEntityConfiguration>(decodedJwt);
            conf.Metadata.OpenIdProvider = OpenIdConnectConfiguration.Create(JObject.Parse(decodedJwt)["metadata"]["openid_provider"].ToString());
            conf.Metadata.OpenIdProvider.JsonWebKeySet = JsonWebKeySet.Create(JObject.Parse(decodedJwt)["metadata"]["openid_provider"]["jwks"].ToString());
            result.Add(new SpidIdentityProvider()
            {
                EntityConfiguration = conf,
                MetadataAddress = metadataAddress,
                Name = conf.Metadata.OpenIdProvider.AdditionalData["op_uri"] as string,
                OrganizationDisplayName = conf.Metadata.OpenIdProvider.AdditionalData["op_name"] as string,
                OrganizationUrl = conf.Metadata.OpenIdProvider.AdditionalData["op_uri"] as string,
                OrganizationLogoUrl = conf.Metadata.OpenIdProvider.AdditionalData["logo_uri"] as string,
                OrganizationName = conf.Metadata.OpenIdProvider.AdditionalData["organization_name"] as string,
                SupportedAcrValues = conf.Metadata.OpenIdProvider.AcrValuesSupported.ToArray(),
            });
        }

        return await Task.FromResult(result);
    }
}
