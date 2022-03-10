using JWT.Algorithms;
using JWT.Builder;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Spid.Cie.OIDC.AspNetCore.Configuration;
using Spid.Cie.OIDC.AspNetCore.Logging;
using Spid.Cie.OIDC.AspNetCore.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Spid.Cie.OIDC.AspNetCore.Events;

internal class SpidCieEvents : OpenIdConnectEvents
{
    private readonly IOptionsMonitor<SpidCieOptions> _spidOptions;
    private readonly ILogPersister _logPersister;
    private readonly IRelyingPartySelector _rpSelector;
    private readonly IIdentityProviderSelector _idpSelector;

    public SpidCieEvents(IOptionsMonitor<SpidCieOptions> spidOptions,
        ILogPersister logPersister,
        IIdentityProviderSelector idpSelector,
        IRelyingPartySelector rpSelector)
    {
        _spidOptions = spidOptions;
        _logPersister = logPersister;
        _rpSelector = rpSelector;
        _idpSelector = idpSelector;
    }

    public override async Task RedirectToIdentityProvider(RedirectContext context)
    {
        var identityProvider = await _idpSelector.GetSelectedIdentityProvider()
            ?? throw new System.Exception("The specified IdentityProvider was found");
        var relyingParty = await _rpSelector.GetSelectedRelyingParty()
            ?? throw new System.Exception("No selected RelyingParty was found");

        context.ProtocolMessage.ClientId = relyingParty.ClientId;
        context.ProtocolMessage.AcrValues = identityProvider.SupportedAcrValues.FirstOrDefault(r => r.Equals(relyingParty.AcrValue, System.StringComparison.CurrentCultureIgnoreCase));
        context.ProtocolMessage.SetParameter("request", GenerateJWTRequest(context.ProtocolMessage));

        await base.RedirectToIdentityProvider(context);
    }

    private string GenerateJWTRequest(OpenIdConnectMessage protocolMessage)
        => JwtBuilder.Create()
            .WithAlgorithm(new RS256Algorithm(null, null)) // TODO: Get signing keys
            .AddClaim("client_id", protocolMessage.ClientId)
            .AddClaim("response_type", protocolMessage.ResponseType)
            .AddClaim("scope", protocolMessage.Scope)
            .AddClaim("code_challenge", protocolMessage.GetParameter("code_challenge"))
            .AddClaim("code_challenge_method", protocolMessage.GetParameter("code_challenge_method"))
            .AddClaim("nonce", protocolMessage.Nonce)
            .AddClaim("prompt", protocolMessage.Prompt)
            .AddClaim("redirect_uri", protocolMessage.RedirectUri)
            .AddClaim("acr_values", protocolMessage.AcrValues)
            .AddClaim("state", protocolMessage.State)
            .AddClaim("claims", new
            {
                userinfo = new Dictionary<string, string>()
            })
            .Encode();

    public override Task RedirectToIdentityProviderForSignOut(RedirectContext context)
    {
        return base.RedirectToIdentityProviderForSignOut(context);
    }

    public override Task MessageReceived(MessageReceivedContext context)
    {
        return base.MessageReceived(context);
    }

}
