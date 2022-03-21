using Microsoft.IdentityModel.Tokens;
using Spid.Cie.OIDC.AspNetCore.Models;
using Spid.Cie.OIDC.AspNetCore.Resources;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Spid.Cie.OIDC.AspNetCore.Services;

internal class TokenValidationParametersRetriever : ITokenValidationParametersRetriever
{
    private readonly IRelyingPartySelector _rpSelector;
    private readonly IIdentityProviderSelector _idpSelector;

    public TokenValidationParametersRetriever(IIdentityProviderSelector idpSelector,
        IRelyingPartySelector rpSelector)
    {
        _rpSelector = rpSelector;
        _idpSelector = idpSelector;
    }

    public async Task<TokenValidationParameters> RetrieveTokenValidationParameter()
    {
        var identityProvider = await _idpSelector.GetSelectedIdentityProvider()
            ?? throw new Exception(ErrorLocalization.IdentityProviderNotFound);
        var relyingParty = await _rpSelector.GetSelectedRelyingParty()
            ?? throw new Exception(ErrorLocalization.RelyingPartyNotFound);

        return new TokenValidationParameters
        {
            NameClaimType = SpidCieConst.Sub,
            ClockSkew = TimeSpan.FromMinutes(5),
            IssuerSigningKeys = identityProvider.EntityConfiguration.Metadata.OpenIdProvider.JsonWebKeySet?.Keys?.ToArray()
                    ?? identityProvider.EntityConfiguration.JWKS?.Keys.Select(k => new Microsoft.IdentityModel.Tokens.JsonWebKey(JsonSerializer.Serialize(k))),
            RequireSignedTokens = true,
            RequireExpirationTime = true,
            ValidateLifetime = true,
            ValidateAudience = true,
            ValidAudience = relyingParty.ClientId,
            ValidateIssuer = true,
            ValidIssuer = identityProvider.EntityConfiguration.Issuer
        };
    }
}
