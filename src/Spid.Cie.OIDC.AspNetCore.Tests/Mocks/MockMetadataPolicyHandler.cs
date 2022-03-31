using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Newtonsoft.Json.Linq;
using Spid.Cie.OIDC.AspNetCore.Services;

namespace Spid.Cie.OIDC.AspNetCore.Tests.Mocks;

internal class MockMetadataPolicyHandler : IMetadataPolicyHandler
{
    public OpenIdConnectConfiguration? ApplyMetadataPolicy(string opDecodedJwt, string metadataPolicy)
    {
        var openIdConfigurationObj = JObject.Parse(opDecodedJwt)["metadata"]["openid_provider"];
        return OpenIdConnectConfiguration.Create(openIdConfigurationObj.ToString());
    }
}
