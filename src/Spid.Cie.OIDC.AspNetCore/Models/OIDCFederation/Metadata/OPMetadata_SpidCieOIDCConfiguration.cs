using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Spid.Cie.OIDC.AspNetCore.Models;

[ExcludeFromCodeCoverage]
sealed class OPMetadata_SpidCieOIDCConfiguration
{
    [JsonIgnore]
    public OpenIdConnectConfiguration? OpenIdProvider { get; set; }
}