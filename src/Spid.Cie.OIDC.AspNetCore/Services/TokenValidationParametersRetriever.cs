using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.IdentityModel.Tokens;
using Spid.Cie.OIDC.AspNetCore.Helpers;
using Spid.Cie.OIDC.AspNetCore.Resources;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Spid.Cie.OIDC.AspNetCore.Services;

class TokenValidationParametersRetriever : ITokenValidationParametersRetriever
{
    readonly IAggregatorsHandler _aggHandler;
    readonly IRelyingPartySelector _rpSelector;
    readonly IIdentityProviderSelector _idpSelector;
    readonly IHttpContextAccessor _httpContextAccessor;

    public TokenValidationParametersRetriever(IIdentityProviderSelector idpSelector, IRelyingPartySelector rpSelector, IAggregatorsHandler aggHandler,
                                                IHttpContextAccessor httpContextAccessor)
    {
        _aggHandler = aggHandler;
        _rpSelector = rpSelector;
        _idpSelector = idpSelector;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<TokenValidationParameters> RetrieveTokenValidationParameter()
    {
        var identityProvider = await _idpSelector.GetSelectedIdentityProvider();
        Throw<Exception>.If(identityProvider is null, ErrorLocalization.IdentityProviderNotFound);

        var relyingParty = await _rpSelector.GetSelectedRelyingParty();

        if (relyingParty == default)
        {
            var aggregators = await _aggHandler.GetAggregators();
            var uri = new Uri(UriHelper.GetEncodedUrl(_httpContextAccessor.HttpContext.Request))
                            .GetLeftPart(UriPartial.Path)
                            .Replace(SpidCieConst.JsonEntityConfigurationPath, "")
                            .Replace(SpidCieConst.EntityConfigurationPath, "")
                            .Replace(SpidCieConst.CallbackPath, "")
                            .Replace(SpidCieConst.SignedOutCallbackPath, "")
                            .Replace(SpidCieConst.RemoteSignOutPath, "")
                            .EnsureTrailingSlash();

            relyingParty = aggregators.SelectMany(a => a.RelyingParties)
                            .OrderByDescending(r => r.Id.Length)
                            .FirstOrDefault(r => uri.StartsWith(r.Id.EnsureTrailingSlash(), StringComparison.OrdinalIgnoreCase));
        }

        Throw<Exception>.If(relyingParty is null, ErrorLocalization.RelyingPartyNotFound);

        return new TokenValidationParameters
        {
            NameClaimType = SpidCieConst.Sub,
            ClockSkew = TimeSpan.FromMinutes(5),
            IssuerSigningKeys = identityProvider!.EntityConfiguration.Metadata.OpenIdProvider!.JsonWebKeySet.Keys.ToArray(),
            RequireSignedTokens = true,
            RequireExpirationTime = true,
            ValidateLifetime = true,
            ValidateAudience = true,
            ValidAudience = relyingParty!.Id,
            ValidateIssuer = true,
            ValidIssuer = identityProvider.EntityConfiguration.Issuer
        };
    }
}