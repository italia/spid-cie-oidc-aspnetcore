using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Spid.Cie.OIDC.AspNetCore.Configuration;
using Spid.Cie.OIDC.AspNetCore.Helpers;
using Spid.Cie.OIDC.AspNetCore.Models;
using Spid.Cie.OIDC.AspNetCore.Resources;
using Spid.Cie.OIDC.AspNetCore.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
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
        var identityProvider = await _idpSelector.GetSelectedIdentityProvider();
        Throw<Exception>.If(identityProvider is null, ErrorLocalization.IdentityProviderNotFound);

        var relyingParty = await _rpSelector.GetSelectedRelyingParty();
        Throw<Exception>.If(relyingParty is null, ErrorLocalization.RelyingPartyNotFound);

        context.ProtocolMessage.IssuerAddress = identityProvider!.EntityConfiguration.Metadata.OpenIdProvider!.AuthorizationEndpoint;
        context.ProtocolMessage.ClientId = relyingParty!.ClientId;
        context.ProtocolMessage.AcrValues = identityProvider.GetAcrValue(relyingParty.SecurityLevel);

        if (_options.CurrentValue.RequestRefreshToken)
            context.ProtocolMessage.Scope += $" {SpidCieConst.OfflineScope}";

        context.Properties.Items[SpidCieConst.IdPSelectorKey] = identityProvider.Uri;
        context.Properties.Items[SpidCieConst.RPSelectorKey] = relyingParty.ClientId;

        await base.RedirectToIdentityProvider(context);
    }

    public virtual async Task PostStateCreated(PostStateCreatedContext context)
    {
        var identityProvider = await _idpSelector.GetSelectedIdentityProvider();
        Throw<Exception>.If(identityProvider is null, ErrorLocalization.IdentityProviderNotFound);

        var relyingParty = await _rpSelector.GetSelectedRelyingParty();
        Throw<Exception>.If(relyingParty is null, ErrorLocalization.RelyingPartyNotFound);

        Throw<Exception>.If(relyingParty!.OpenIdCoreCertificates is null || relyingParty!.OpenIdCoreCertificates.Count() == 0,
                "No OpenIdCore certificates were found in the currently selected RelyingParty");
        var certificate = relyingParty!.OpenIdCoreCertificates!.FirstOrDefault()!;

        context.ProtocolMessage.SetParameter(SpidCieConst.RequestParameter,
            GenerateJWTRequest(identityProvider!, relyingParty!, context.ProtocolMessage, certificate));
    }

    private string GenerateJWTRequest(IdentityProvider idp,
        RelyingParty relyingParty,
        OpenIdConnectMessage protocolMessage,
        X509Certificate2 certificate)
    {
        return _cryptoService.CreateJWT(certificate!,
            new Dictionary<string, object>() {
                    { SpidCieConst.Iss, protocolMessage.ClientId },
                    { SpidCieConst.Sub, protocolMessage.ClientId },
                    { SpidCieConst.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
                    { SpidCieConst.Exp, DateTimeOffset.UtcNow.AddMinutes(SpidCieConst.EntityConfigurationExpirationInMinutes).ToUnixTimeSeconds() },
                    { SpidCieConst.Aud, new string[] { idp.EntityConfiguration.Issuer, idp.EntityConfiguration.Metadata.OpenIdProvider!.AuthorizationEndpoint } },
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

            context.Options.TokenValidationParameters = await _tokenValidationParametersRetriever.RetrieveTokenValidationParameter();
        }
        await base.MessageReceived(context);
    }

    public override async Task AuthorizationCodeReceived(AuthorizationCodeReceivedContext context)
    {
        var identityProvider = await _idpSelector.GetSelectedIdentityProvider();
        Throw<Exception>.If(identityProvider is null, ErrorLocalization.IdentityProviderNotFound);

        var relyingParty = await _rpSelector.GetSelectedRelyingParty();
        Throw<Exception>.If(relyingParty is null, ErrorLocalization.RelyingPartyNotFound);
        Throw<Exception>.If(relyingParty!.OpenIdCoreCertificates is null || relyingParty!.OpenIdCoreCertificates.Count() == 0,
                "No OpenIdCore Keys were found in the currently selected RelyingParty");

        var certificate = relyingParty!.OpenIdCoreCertificates!.FirstOrDefault()!;

        Throw<Exception>.If(context.TokenEndpointRequest is null, $"No Token Endpoint Request found in the current context");

        context.TokenEndpointRequest!.ClientAssertionType = SpidCieConst.ClientAssertionTypeValue;
        context.TokenEndpointRequest!.ClientAssertion = _cryptoService.CreateClientAssertion(identityProvider!, relyingParty.ClientId!, certificate);

        await base.AuthorizationCodeReceived(context);
    }
}
