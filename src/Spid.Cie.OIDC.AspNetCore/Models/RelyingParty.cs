using Microsoft.IdentityModel.Tokens;
using Spid.Cie.OIDC.AspNetCore.Helpers;
using System;

namespace Spid.Cie.OIDC.AspNetCore.Models;

public sealed class RelyingParty
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
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
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    internal RPEntityConfiguration EntityConfiguration
    {
        get
        {
            return new RPEntityConfiguration()
            {
                ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(SpidCieConst.EntityConfigurationExpirationInMinutes),
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
                            ? new[] { SpidCieConst.AuthorizationCode, SpidCieConst.RefreshToken }
                            : new[] { SpidCieConst.AuthorizationCode },
                        JWKS = OpenIdCoreJWKs.GetJWKS(),
                        RedirectUris = RedirectUris,
                        ResponseTypes = new[] { SpidCieConst.ResponseType }
                    }
                }
            };
        }
    }
}
