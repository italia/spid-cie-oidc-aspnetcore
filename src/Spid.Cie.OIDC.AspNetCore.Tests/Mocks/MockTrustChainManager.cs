using Spid.Cie.OIDC.AspNetCore.Models;
using Spid.Cie.OIDC.AspNetCore.Services;
using System.Threading.Tasks;

namespace Spid.Cie.OIDC.AspNetCore.Tests.Mocks;

internal class MockTrustChainManager : ITrustChainManager
{
    public async Task<IdPEntityConfiguration?> BuildTrustChain(string url)
    {
        await Task.CompletedTask;
        var result = new IdPEntityConfiguration()
        {
            Issuer = url,
            Metadata = new IdPMetadata_SpidCieOIDCConfiguration()
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

    public TrustChain? GetResolvedTrustChain(string sub, string anchor)
    {
        return new TrustChain()
        {
            ExpiresOn = System.DateTimeOffset.MaxValue,
            Chain = new System.Collections.Generic.List<string> { "test1", "test2" },
            OpConf = new IdPEntityConfiguration()
            {
                Issuer = sub,
                Metadata = new IdPMetadata_SpidCieOIDCConfiguration()
                {
                    OpenIdProvider = new Microsoft.IdentityModel.Protocols.OpenIdConnect.OpenIdConnectConfiguration()
                }
            },
            TrustAnchorUsed = anchor
        };
    }
}
