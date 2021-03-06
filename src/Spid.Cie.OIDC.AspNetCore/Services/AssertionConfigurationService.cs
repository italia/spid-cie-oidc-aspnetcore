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
using System.Linq;
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

        Throw<Exception>.If(rp!.OpenIdCoreCertificates is null || rp!.OpenIdCoreCertificates.Count() == 0,
                "No OpenIdCore Keys were found in the currently selected RelyingParty");
        var certificate = rp!.OpenIdCoreCertificates!.FirstOrDefault()!;

        return new ClientAssertion()
        {
            Type = SpidCieConst.ClientAssertionTypeValue,
            Value = _cryptoService.CreateClientAssertion(idp!, clientId!, certificate)
        };
    }
}
