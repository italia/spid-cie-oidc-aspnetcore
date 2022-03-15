using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Spid.Cie.OIDC.AspNetCore.Helpers;
using Spid.Cie.OIDC.AspNetCore.Models;
using Spid.Cie.OIDC.AspNetCore.Resources;
using Spid.Cie.OIDC.AspNetCore.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;

namespace Spid.Cie.OIDC.AspNetCore.Events;

internal class SpidCieEvents : OpenIdConnectEvents
{
    private readonly IRelyingPartySelector _rpSelector;
    private readonly IIdentityProviderSelector _idpSelector;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SpidCieEvents(IIdentityProviderSelector idpSelector,
        IRelyingPartySelector rpSelector,
        IHttpContextAccessor httpContextAccessor)
    {
        _rpSelector = rpSelector;
        _idpSelector = idpSelector;
        _httpContextAccessor = httpContextAccessor;
    }


    public override async Task RedirectToIdentityProvider(RedirectContext context)
    {
        var identityProvider = await _idpSelector.GetSelectedIdentityProvider()
            ?? throw new System.Exception(ErrorLocalization.IdentityProviderNotFound);
        var relyingParty = await _rpSelector.GetSelectedRelyingParty()
            ?? throw new System.Exception(ErrorLocalization.RelyingPartyNotFound);

        context.ProtocolMessage.IssuerAddress = identityProvider.EntityConfiguration.Metadata.OpenIdProvider.AuthorizationEndpoint;
        context.ProtocolMessage.ClientId = relyingParty.ClientId;
        context.ProtocolMessage.AcrValues = identityProvider.GetAcrValue(relyingParty.SecurityLevel);

        context.Properties.Items[SpidCieDefaults.IdPSelectorKey] = identityProvider.Name;
        context.Properties.Items[SpidCieDefaults.RPSelectorKey] = relyingParty.ClientId;

        await base.RedirectToIdentityProvider(context);
    }

    public virtual async Task PostStateCreated(PostStateCreatedContext context)
    {
        var identityProvider = await _idpSelector.GetSelectedIdentityProvider()
            ?? throw new System.Exception(ErrorLocalization.IdentityProviderNotFound);
        var relyingParty = await _rpSelector.GetSelectedRelyingParty()
            ?? throw new System.Exception(ErrorLocalization.RelyingPartyNotFound);

        context.ProtocolMessage.SetParameter(SpidCieDefaults.RequestParameter,
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
            RSA rsa = key.GetRSAKey();

            return CryptoHelpers.CreateJWT(rsa,
                new Dictionary<string, object>() {
                    { SpidCieDefaults.Kid, key.Kid },
                    { SpidCieDefaults.Typ, SpidCieDefaults.TypValue }
                },
                new Dictionary<string, object>() {
                    { SpidCieDefaults.Iss, protocolMessage.ClientId },
                    { SpidCieDefaults.Sub, protocolMessage.ClientId },
                    { SpidCieDefaults.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
                    { SpidCieDefaults.Exp, DateTimeOffset.UtcNow.AddMinutes(SpidCieDefaults.EntityConfigurationExpirationInMinutes).ToUnixTimeSeconds() },
                    { SpidCieDefaults.Aud, new string[] { idp.EntityConfiguration.Issuer, idp.EntityConfiguration.Metadata.OpenIdProvider.AuthorizationEndpoint } },
                    { SpidCieDefaults.ClientId, protocolMessage.ClientId },
                    { SpidCieDefaults.ResponseTypeParameter, protocolMessage.ResponseType },
                    { SpidCieDefaults.Scope, protocolMessage.Scope },
                    { SpidCieDefaults.CodeChallenge, protocolMessage.GetParameter(SpidCieDefaults.CodeChallenge) },
                    { SpidCieDefaults.CodeChallengeMethod, protocolMessage.GetParameter(SpidCieDefaults.CodeChallengeMethod) },
                    { SpidCieDefaults.Nonce, protocolMessage.Nonce },
                    { SpidCieDefaults.PromptParameter, protocolMessage.Prompt },
                    { SpidCieDefaults.RedirectUri, protocolMessage.RedirectUri },
                    { SpidCieDefaults.AcrValues, protocolMessage.AcrValues },
                    { SpidCieDefaults.State, protocolMessage.State },
                    { SpidCieDefaults.Claims, new { userinfo = idp.FilterRequestedClaims(relyingParty.RequestedClaims).ToDictionary(c => c, c => (object)null) } }
                });

        }
        throw new System.Exception(ErrorLocalization.NoSigningKeyFound);
    }

    public override async Task MessageReceived(MessageReceivedContext context)
    {
        context.Properties.Items.TryGetValue(SpidCieDefaults.IdPSelectorKey, out var provider);
        if (!string.IsNullOrWhiteSpace(provider))
        {
            _httpContextAccessor.HttpContext.Items.Add(SpidCieDefaults.IdPSelectorKey, provider);
        }

        context.Properties.Items.TryGetValue(SpidCieDefaults.RPSelectorKey, out var clientId);
        if (!string.IsNullOrWhiteSpace(clientId))
        {
            context.Options.ClientId = clientId;
            _httpContextAccessor.HttpContext.Items.Add(SpidCieDefaults.RPSelectorKey, clientId);
        }

        var identityProvider = await _idpSelector.GetSelectedIdentityProvider()
            ?? throw new System.Exception(ErrorLocalization.IdentityProviderNotFound);
        var relyingParty = await _rpSelector.GetSelectedRelyingParty()
            ?? throw new System.Exception(ErrorLocalization.RelyingPartyNotFound);

        context.Options.TokenValidationParameters = new TokenValidationParameters
        {
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

        await base.MessageReceived(context);
    }

    public override async Task AuthorizationCodeReceived(AuthorizationCodeReceivedContext context)
    {
        context.TokenEndpointRequest.ClientAssertionType = SpidCieDefaults.ClientAssertionType;
        var identityProvider = await _idpSelector.GetSelectedIdentityProvider()
            ?? throw new System.Exception(ErrorLocalization.IdentityProviderNotFound);
        var relyingParty = await _rpSelector.GetSelectedRelyingParty()
            ?? throw new System.Exception(ErrorLocalization.RelyingPartyNotFound);

        var keySet = relyingParty.OpenIdCoreJWKs;
        var key = keySet?.Keys?.FirstOrDefault();
        if (key is not null)
        {
            RSA rsa = key.GetRSAKey();

            context.TokenEndpointRequest.ClientAssertion = CryptoHelpers.CreateJWT(rsa,
                new Dictionary<string, object>() {
                    { SpidCieDefaults.Kid, key.Kid },
                    { SpidCieDefaults.Typ, SpidCieDefaults.TypValue }
                },
                new Dictionary<string, object>() {
                    { SpidCieDefaults.Iss, relyingParty.ClientId },
                    { SpidCieDefaults.Sub, relyingParty.ClientId },
                    { SpidCieDefaults.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
                    { SpidCieDefaults.Exp, DateTimeOffset.UtcNow.AddMinutes(SpidCieDefaults.EntityConfigurationExpirationInMinutes).ToUnixTimeSeconds() },
                    { SpidCieDefaults.Aud, new string[] { identityProvider.EntityConfiguration.Metadata.OpenIdProvider.TokenEndpoint } },
                    { SpidCieDefaults.Jti, Guid.NewGuid().ToString() }
                });
        }

        await base.AuthorizationCodeReceived(context);
    }
}
