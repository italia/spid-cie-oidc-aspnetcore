using Microsoft.IdentityModel.Tokens;
using Spid.Cie.OIDC.AspNetCore.Helpers;
using System;

namespace Spid.Cie.OIDC.AspNetCore.Models;

public sealed class RelyingParty
{
    public string ClientId { get; set; }
    public string ClientName { get; set; }
    public SecurityLevel SecurityLevel { get; set; }
    public string[] AuthorityHints { get; set; }
    public string Issuer { get; set; }
    public TrustMarkDefinition[] TrustMarks { get; set; }
    public JsonWebKeySet OpenIdFederationJWKs { get; set; }
    public JsonWebKeySet OpenIdCoreJWKs { get; set; }
    public string[] Contacts { get; set; }
    public bool LongSessionsEnabled { get; set; }
    public string[] RedirectUris { get; set; }
    public ClaimTypes[] RequestedClaims { get; set; }

    internal RPEntityConfiguration EntityConfiguration
    {
        get
        {
            return new RPEntityConfiguration()
            {
                ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(SpidCieDefaults.EntityConfigurationExpirationInMinutes),
                IssuedAt = DateTimeOffset.UtcNow,
                AuthorityHints = AuthorityHints,
                Issuer = Issuer,
                Subject = ClientId,
                TrustMarks = TrustMarks,
                JWKS = OpenIdFederationJWKs.GetJWKS(),
                Metadata = new RPMetadata_SpidCieOIDCConfiguration()
                {
                    OpenIdRelyingParty = new RP_SpidCieOIDCConfiguration()
                    {
                        ClientName = ClientName,
                        Contacts = Contacts,
                        GrantTypes = LongSessionsEnabled
                            ? new[] { SpidCieDefaults.AuthorizationCode, SpidCieDefaults.RefreshToken }
                            : new[] { SpidCieDefaults.AuthorizationCode },
                        JWKS = OpenIdCoreJWKs.GetJWKS(),
                        RedirectUris = RedirectUris,
                        ResponseTypes = new[] { SpidCieDefaults.ResponseType }
                    }
                }
            };
        }
    }
}
