using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Spid.Cie.OIDC.AspNetCore.Services;

interface IMetadataPolicyHandler
{
    OpenIdConnectConfiguration? ApplyMetadataPolicy(string opDecodedJwt, string metadataPolicy);
}
