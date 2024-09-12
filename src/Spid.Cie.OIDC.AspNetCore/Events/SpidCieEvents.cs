using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Spid.Cie.OIDC.AspNetCore.Configuration;
using Spid.Cie.OIDC.AspNetCore.Enums;
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
    readonly ICryptoService _cryptoService;
    readonly IAggregatorsHandler _aggHandler;
    readonly IRelyingPartySelector _rpSelector;
    readonly IIdentityProviderSelector _idpSelector;
    readonly IOptionsMonitor<SpidCieOptions> _options;
    readonly IHttpContextAccessor _httpContextAccessor;
    readonly ITokenValidationParametersRetriever _tokenValidationParametersRetriever;

    public SpidCieEvents(IOptionsMonitor<SpidCieOptions> options, IIdentityProviderSelector idpSelector, IRelyingPartySelector rpSelector, ICryptoService cryptoService,
                            IAggregatorsHandler aggHandler, ITokenValidationParametersRetriever tokenValidationParametersRetriever, IHttpContextAccessor httpContextAccessor)
    {
        _options = options;
        _rpSelector = rpSelector;
        _aggHandler = aggHandler;
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

        context.ProtocolMessage.AuthorizationEndpoint = identityProvider?.EntityConfiguration?.Metadata?.OpenIdProvider?.AuthorizationEndpoint!;
        context.ProtocolMessage.IssuerAddress = identityProvider?.EntityConfiguration?.Metadata?.OpenIdProvider?.AuthorizationEndpoint!;
        context.ProtocolMessage.TokenEndpoint = identityProvider?.EntityConfiguration?.Metadata?.OpenIdProvider?.TokenEndpoint!;
        context.ProtocolMessage.ClientId = relyingParty!.Id;
        context.ProtocolMessage.RedirectUri = $"{relyingParty!.Id.RemoveTrailingSlash()}{SpidCieConst.CallbackPath}";
        context.ProtocolMessage.AcrValues = identityProvider.GetAcrValue(relyingParty.SecurityLevel);

        if (_options.CurrentValue.RequestRefreshToken)
            context.ProtocolMessage.Scope += $" {SpidCieConst.OfflineScope}";

        context.Properties.Items[SpidCieConst.IdPSelectorKey] = identityProvider.Uri;
        context.Properties.Items[SpidCieConst.RPSelectorKey] = relyingParty.Id;

        await base.RedirectToIdentityProvider(context);
    }

    public virtual async Task PostStateCreated(PostStateCreatedContext context)
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
        Throw<Exception>.If(relyingParty!.OpenIdCoreCertificates is null || relyingParty!.OpenIdCoreCertificates.Count() == 0,
                "No OpenIdCore certificates were found in the currently selected RelyingParty");

        var certificate = relyingParty!.OpenIdCoreCertificates!.FirstOrDefault(occ => occ.KeyUsage == KeyUsageTypes.Signature)!;

        context.ProtocolMessage.SetParameter(SpidCieConst.RequestParameter,
            GenerateJWTRequest(identityProvider!, relyingParty!, context.ProtocolMessage, certificate.Certificate!));
    }

    string GenerateJWTRequest(IdentityProvider idp, RelyingParty relyingParty, OpenIdConnectMessage protocolMessage, X509Certificate2 certificate)
    {
        return _cryptoService.CreateJWT(certificate!,
            new Dictionary<string, object>() {
                    { SpidCieConst.Iss, protocolMessage.ClientId },
                    //{ SpidCieConst.Sub, protocolMessage.ClientId },
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
        if (!string.IsNullOrWhiteSpace(context.ProtocolMessage!.Error))
        {
            var ex = new Exception(context.ProtocolMessage.ErrorDescription ?? context.ProtocolMessage.Error);

            ex.Data.Add(nameof(context.ProtocolMessage.Error), context.ProtocolMessage.Error);
            ex.Data.Add(nameof(context.ProtocolMessage.ErrorDescription), context.ProtocolMessage.ErrorDescription);
            ex.Data.Add(nameof(context.ProtocolMessage.ErrorUri), context.ProtocolMessage.ErrorUri);
            context.Fail(ex);
        }
        else
        {
            context.Properties!.Items.TryGetValue(SpidCieConst.IdPSelectorKey, out var provider);

            if (!string.IsNullOrWhiteSpace(provider))
                _httpContextAccessor.HttpContext!.Items.Add(SpidCieConst.IdPSelectorKey, provider);

            context.Properties!.Items.TryGetValue(SpidCieConst.RPSelectorKey, out var clientId);

            if (!string.IsNullOrWhiteSpace(clientId))
            {
                context.Options.ClientId = clientId;
                _httpContextAccessor.HttpContext!.Items.Add(SpidCieConst.RPSelectorKey, clientId);
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
        Throw<Exception>.If(relyingParty!.OpenIdCoreCertificates is null || relyingParty!.OpenIdCoreCertificates.Count() == 0,
                "No OpenIdCore Keys were found in the currently selected RelyingParty");

        var certificate = relyingParty!.OpenIdCoreCertificates!.FirstOrDefault(occ => occ.KeyUsage == KeyUsageTypes.Signature)!;

        Throw<Exception>.If(context.TokenEndpointRequest is null, $"No Token Endpoint Request found in the current context");

        context.TokenEndpointRequest!.ClientAssertionType = SpidCieConst.ClientAssertionTypeValue;
        context.TokenEndpointRequest!.ClientAssertion = _cryptoService.CreateClientAssertion(identityProvider!.EntityConfiguration.Metadata.OpenIdProvider!.TokenEndpoint!,
            relyingParty.Id!, certificate.Certificate!);

        await base.AuthorizationCodeReceived(context);
    }
}