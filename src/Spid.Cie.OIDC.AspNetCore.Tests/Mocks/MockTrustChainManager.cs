using Spid.Cie.OIDC.AspNetCore.Models;
using Spid.Cie.OIDC.AspNetCore.Services;
using System.Threading.Tasks;

namespace Spid.Cie.OIDC.AspNetCore.Tests.Mocks;

class MockTrustChainManager : ITrustChainManager
{
    public async Task<OPEntityConfiguration?> BuildTrustChain(string url)
    {
        await Task.CompletedTask;

        var result = new OPEntityConfiguration()
        {
            Issuer = url,
            Metadata = new OPMetadata_SpidCieOIDCConfiguration()
            {
                OpenIdProvider = new Microsoft.IdentityModel.Protocols.OpenIdConnect.OpenIdConnectConfiguration()
            }
        };

        result.Metadata.OpenIdProvider.AdditionalData.Add("op_uri", "test");
        result.Metadata.OpenIdProvider.AdditionalData.Add("op_name", "test");
        result.Metadata.OpenIdProvider.AdditionalData.Add("logo_uri", "test");
        result.Metadata.OpenIdProvider.AdditionalData.Add("organization_name", "test");
        result.Metadata.OpenIdProvider.AcrValuesSupported.Add("test");

        return result;
    }

    public async Task<RPEntityConfiguration?> BuildRPTrustChain(string url)
    {
        await Task.CompletedTask;

        var result = new RPEntityConfiguration()
        {
            Issuer = url,
            Metadata = new RPMetadata_SpidCieOIDCConfiguration()
            {
                FederationEntity = new RP_SpidCieOIDCFederationEntity
                {

                },
                OpenIdRelyingParty = new RP_SpidCieOIDCConfiguration
                {

                }
            }
        };

        return result;
    }

    public TrustChain<T>? GetResolvedTrustChain<T>(string sub, string anchor) where T : EntityConfiguration
    {
        return typeof(T).Equals(typeof(OPEntityConfiguration)) ? new OPEntityConfiguration()
        {
            ExpiresOn = System.DateTimeOffset.MaxValue,
        } as TrustChain<T> : typeof(T).Equals(typeof(RPEntityConfiguration)) ? new RPEntityConfiguration
        {
            ExpiresOn = System.DateTimeOffset.MaxValue,
        } as TrustChain<T> : default;
    }
}