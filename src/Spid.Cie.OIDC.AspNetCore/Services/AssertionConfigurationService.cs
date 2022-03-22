using IdentityModel.AspNetCore.AccessTokenManagement;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Spid.Cie.OIDC.AspNetCore.Helpers;
using Spid.Cie.OIDC.AspNetCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Spid.Cie.OIDC.AspNetCore.Services;

internal class AssertionConfigurationService : DefaultTokenClientConfigurationService
{
    private readonly IRelyingPartiesHandler _rpRetriever;
    private readonly IIdentityProvidersHandler _idpHandler;
    private readonly ICryptoService _cryptoService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AssertionConfigurationService(IHttpContextAccessor httpContextAccessor,
            IRelyingPartiesHandler rpRetriever,
            IIdentityProvidersHandler idpHandler,
            UserAccessTokenManagementOptions userAccessTokenManagementOptions,
            ClientAccessTokenManagementOptions clientAccessTokenManagementOptions,
            IOptionsMonitor<OpenIdConnectOptions> oidcOptions,
            IAuthenticationSchemeProvider schemeProvider,
            ICryptoService cryptoService,
            ILogger<DefaultTokenClientConfigurationService> logger)
        : base(userAccessTokenManagementOptions,
            clientAccessTokenManagementOptions,
            oidcOptions,
            schemeProvider,
            logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _rpRetriever = rpRetriever;
        _idpHandler = idpHandler;
        _cryptoService = cryptoService;
    }

    protected override async Task<ClientAssertion> CreateAssertionAsync(string? clientName = null)
    {
        var issuer = _httpContextAccessor.HttpContext!.User.FindFirst(SpidCieConst.Iss)?.Value;
        Throw<InvalidOperationException>.If(string.IsNullOrWhiteSpace(issuer),
            "Current authenticated User doesn't have a 'sub' claim.");

        var idps = await _idpHandler.GetIdentityProviders();
        var idp = idps.FirstOrDefault(i => i.EntityConfiguration.Issuer.Equals(issuer));
        Throw<InvalidOperationException>.If(idp is null,
            $"No IdentityProvider found for the issuer {issuer}");

        var clientId = _httpContextAccessor.HttpContext!.User.FindFirst(SpidCieConst.Aud)?.Value;
        Throw<InvalidOperationException>.If(string.IsNullOrWhiteSpace(clientId),
            "Current authenticated User doesn't have an 'aud' claim.");

        var rps = await _rpRetriever.GetRelyingParties();
        var rp = rps.FirstOrDefault(r => r.ClientId.Equals(clientId));
        Throw<InvalidOperationException>.If(rp is null,
            $"No RelyingParty found for the clientId {clientId}");

        var keySet = rp!.OpenIdCoreJWKs;
        var key = keySet?.Keys?.FirstOrDefault();
        Throw<InvalidOperationException>.If(key is null,
            $"No key found for the RelyingParty with clientId {clientId}");

        (RSA publicKey, RSA privateKey) = _cryptoService.GetRSAKeys(key!);

        return new ClientAssertion()
        {
            Type = SpidCieConst.ClientAssertionTypeValue,
            Value = _cryptoService.CreateJWT(publicKey,
                    privateKey,
                    new Dictionary<string, object>() {
                        { SpidCieConst.Kid, key!.Kid },
                        { SpidCieConst.Typ, SpidCieConst.TypValue }
                    },
                    new Dictionary<string, object>() {
                        { SpidCieConst.Iss, clientId! },
                        { SpidCieConst.Sub, clientId! },
                        { SpidCieConst.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
                        { SpidCieConst.Exp, DateTimeOffset.UtcNow.AddMinutes(SpidCieConst.EntityConfigurationExpirationInMinutes).ToUnixTimeSeconds() },
                        { SpidCieConst.Aud, new string[] { idp!.EntityConfiguration.Metadata.OpenIdProvider.TokenEndpoint } },
                        { SpidCieConst.Jti, Guid.NewGuid().ToString() }
                    })
        };
    }
}
