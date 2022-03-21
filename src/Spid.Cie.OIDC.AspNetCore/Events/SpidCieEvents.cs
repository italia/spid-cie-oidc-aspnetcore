using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Spid.Cie.OIDC.AspNetCore.Configuration;
using Spid.Cie.OIDC.AspNetCore.Helpers;
using Spid.Cie.OIDC.AspNetCore.Models;
using Spid.Cie.OIDC.AspNetCore.Resources;
using Spid.Cie.OIDC.AspNetCore.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Spid.Cie.OIDC.AspNetCore.Events;

internal class SpidCieEvents : OpenIdConnectEvents
{
    private readonly IOptionsMonitor<SpidCieOptions> _options;
    private readonly IRelyingPartySelector _rpSelector;
    private readonly IIdentityProviderSelector _idpSelector;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITokenValidationParametersRetriever _tokenValidationParametersRetriever;
    private readonly ICryptoService _cryptoService;

    public SpidCieEvents(IOptionsMonitor<SpidCieOptions> options,
        IIdentityProviderSelector idpSelector,
        IRelyingPartySelector rpSelector,
        ICryptoService cryptoService,
        ITokenValidationParametersRetriever tokenValidationParametersRetriever,
        IHttpContextAccessor httpContextAccessor)
    {
        _options = options;
        _rpSelector = rpSelector;
        _idpSelector = idpSelector;
        _httpContextAccessor = httpContextAccessor;
        _tokenValidationParametersRetriever = tokenValidationParametersRetriever;
        _cryptoService = cryptoService;
    }


    public override async Task RedirectToIdentityProvider(RedirectContext context)
    {
        var identityProvider = await _idpSelector.GetSelectedIdentityProvider()
            ?? throw new Exception(ErrorLocalization.IdentityProviderNotFound);
        var relyingParty = await _rpSelector.GetSelectedRelyingParty()
            ?? throw new Exception(ErrorLocalization.RelyingPartyNotFound);

        context.ProtocolMessage.IssuerAddress = identityProvider.EntityConfiguration.Metadata.OpenIdProvider.AuthorizationEndpoint;
        context.ProtocolMessage.ClientId = relyingParty.ClientId;
        context.ProtocolMessage.AcrValues = identityProvider.GetAcrValue(relyingParty.SecurityLevel);

        if (_options.CurrentValue.RequestRefreshToken)
            context.ProtocolMessage.Scope += $" {SpidCieConst.OfflineScope}";

        context.Properties.Items[SpidCieConst.IdPSelectorKey] = identityProvider.Uri;
        context.Properties.Items[SpidCieConst.RPSelectorKey] = relyingParty.ClientId;

        await base.RedirectToIdentityProvider(context);
    }

    public virtual async Task PostStateCreated(PostStateCreatedContext context)
    {
        var identityProvider = await _idpSelector.GetSelectedIdentityProvider()
            ?? throw new Exception(ErrorLocalization.IdentityProviderNotFound);
        var relyingParty = await _rpSelector.GetSelectedRelyingParty()
            ?? throw new Exception(ErrorLocalization.RelyingPartyNotFound);

        context.ProtocolMessage.SetParameter(SpidCieConst.RequestParameter,
            GenerateJWTRequest(identityProvider, relyingParty, context.ProtocolMessage, relyingParty.OpenIdCoreJWKs));
    }

    private string GenerateJWTRequest(IdentityProvider idp,
        RelyingParty relyingParty,
        OpenIdConnectMessage protocolMessage,
        JsonWebKeySet keySet)
    {
        var key = keySet?.Keys?.FirstOrDefault();
        if (key is not null)
        {
            (RSA publicKey, RSA privateKey) = _cryptoService.GetRSAKeys(key);

            return _cryptoService.CreateJWT(publicKey,
                privateKey,
                new Dictionary<string, object>() {
                    { SpidCieConst.Kid, key.Kid },
                    { SpidCieConst.Typ, SpidCieConst.TypValue }
                },
                new Dictionary<string, object>() {
                    { SpidCieConst.Iss, protocolMessage.ClientId },
                    { SpidCieConst.Sub, protocolMessage.ClientId },
                    { SpidCieConst.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
                    { SpidCieConst.Exp, DateTimeOffset.UtcNow.AddMinutes(SpidCieConst.EntityConfigurationExpirationInMinutes).ToUnixTimeSeconds() },
                    { SpidCieConst.Aud, new string[] { idp.EntityConfiguration.Issuer, idp.EntityConfiguration.Metadata.OpenIdProvider.AuthorizationEndpoint } },
                    { SpidCieConst.ClientId, protocolMessage.ClientId },
                    { SpidCieConst.ResponseTypeParameter, protocolMessage.ResponseType },
                    { SpidCieConst.Scope, protocolMessage.Scope },
                    { SpidCieConst.CodeChallenge, protocolMessage.GetParameter(SpidCieConst.CodeChallenge) },
                    { SpidCieConst.CodeChallengeMethod, protocolMessage.GetParameter(SpidCieConst.CodeChallengeMethod) },
                    { SpidCieConst.Nonce, protocolMessage.Nonce },
                    { SpidCieConst.PromptParameter, protocolMessage.Prompt },
                    { SpidCieConst.RedirectUri, protocolMessage.RedirectUri },
                    { SpidCieConst.AcrValues, protocolMessage.AcrValues },
                    { SpidCieConst.State, protocolMessage.State },
                    { SpidCieConst.Claims, new { userinfo = idp.FilterRequestedClaims(relyingParty.RequestedClaims).ToDictionary(c => c, c => (object?)null) } }
                });

        }
        throw new Exception(ErrorLocalization.NoSigningKeyFound);
    }

    public override async Task MessageReceived(MessageReceivedContext context)
    {
        if (string.IsNullOrWhiteSpace(context.ProtocolMessage?.Error))
        {
            string? provider = null;
            context.Properties?.Items.TryGetValue(SpidCieConst.IdPSelectorKey, out provider);
            if (!string.IsNullOrWhiteSpace(provider))
            {
                _httpContextAccessor.HttpContext?.Items.Add(SpidCieConst.IdPSelectorKey, provider);
            }

            string? clientId = null;
            context.Properties?.Items.TryGetValue(SpidCieConst.RPSelectorKey, out clientId);
            if (!string.IsNullOrWhiteSpace(clientId))
            {
                context.Options.ClientId = clientId;
                _httpContextAccessor.HttpContext?.Items.Add(SpidCieConst.RPSelectorKey, clientId);
            }

            var identityProvider = await _idpSelector.GetSelectedIdentityProvider()
                ?? throw new Exception(ErrorLocalization.IdentityProviderNotFound);
            var relyingParty = await _rpSelector.GetSelectedRelyingParty()
                ?? throw new Exception(ErrorLocalization.RelyingPartyNotFound);

            context.Options.TokenValidationParameters = await _tokenValidationParametersRetriever.RetrieveTokenValidationParameter();
        }
        await base.MessageReceived(context);
    }

    public override async Task AuthorizationCodeReceived(AuthorizationCodeReceivedContext context)
    {
        var identityProvider = await _idpSelector.GetSelectedIdentityProvider()
            ?? throw new Exception(ErrorLocalization.IdentityProviderNotFound);
        var relyingParty = await _rpSelector.GetSelectedRelyingParty()
            ?? throw new Exception(ErrorLocalization.RelyingPartyNotFound);

        var keySet = relyingParty.OpenIdCoreJWKs;
        var key = keySet?.Keys?.FirstOrDefault();
        if (key is not null)
        {
            (RSA publicKey, RSA privateKey) = _cryptoService.GetRSAKeys(key);

            if (context.TokenEndpointRequest is not null)
            {
                context.TokenEndpointRequest.ClientAssertionType = SpidCieConst.ClientAssertionTypeValue;
                context.TokenEndpointRequest.ClientAssertion = _cryptoService.CreateJWT(publicKey,
                    privateKey,
                    new Dictionary<string, object>() {
                    { SpidCieConst.Kid, key.Kid },
                    { SpidCieConst.Typ, SpidCieConst.TypValue }
                    },
                    new Dictionary<string, object>() {
                    { SpidCieConst.Iss, relyingParty.ClientId },
                    { SpidCieConst.Sub, relyingParty.ClientId },
                    { SpidCieConst.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
                    { SpidCieConst.Exp, DateTimeOffset.UtcNow.AddMinutes(SpidCieConst.EntityConfigurationExpirationInMinutes).ToUnixTimeSeconds() },
                    { SpidCieConst.Aud, new string[] { identityProvider.EntityConfiguration.Metadata.OpenIdProvider.TokenEndpoint } },
                    { SpidCieConst.Jti, Guid.NewGuid().ToString() }
                    });
            }
        }

        await base.AuthorizationCodeReceived(context);
    }
}
