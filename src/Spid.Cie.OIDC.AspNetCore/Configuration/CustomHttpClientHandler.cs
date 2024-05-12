using Spid.Cie.OIDC.AspNetCore.Helpers;
using Spid.Cie.OIDC.AspNetCore.Services;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Spid.Cie.OIDC.AspNetCore.Configuration;

internal class CustomHttpClientHandler : HttpClientHandler
{
    private readonly IRelyingPartySelector _rpSelector;
    private readonly ILogPersister _logPersister;
    private readonly ICryptoService _cryptoService;

    public CustomHttpClientHandler(IRelyingPartySelector rpSelector,
        ILogPersister logPersister,
        ICryptoService cryptoService)
    {
        _rpSelector = rpSelector;
        _logPersister = logPersister;
        _cryptoService = cryptoService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        await _logPersister.LogRequest(request.RequestUri!, request.Content is not null ? await request.Content.ReadAsStringAsync() : null);

        var response = await base.SendAsync(request, cancellationToken);

        await _logPersister.LogResponse(request.RequestUri!, response.StatusCode, await response.Content.ReadAsStringAsync());

        return await DecodeJoseResponse(response);
    }

    public async Task<HttpResponseMessage> DecodeJoseResponse(HttpResponseMessage response)
    {
        if (("application/jose").Equals(response.Content.Headers.ContentType!.MediaType, StringComparison.OrdinalIgnoreCase)
            || ("application/jwt").Equals(response.Content.Headers.ContentType!.MediaType, StringComparison.OrdinalIgnoreCase))
        {
            var token = await response.Content.ReadAsStringAsync();
            Throw<Exception>.If(string.IsNullOrWhiteSpace(token), "No Body Content found in the Jose response");

            Throw<Exception>.If(token.Count(c => c == '.') != 2 && token.Count(c => c == '.') != 4,
                "Invalid Jose response according to https://www.rfc-editor.org/rfc/rfc7516#section-9");

            var provider = await _rpSelector.GetSelectedRelyingParty();
            Throw<Exception>.If(provider is null, "No currently selected RelyingParty was found");
            Throw<Exception>.If(provider!.OpenIdCoreCertificates is null || provider!.OpenIdCoreCertificates.Count() == 0,
                "No OpenIdCore Certificates were found in the currently selected RelyingParty");

            var certificate = provider!.OpenIdCoreCertificates!.FirstOrDefault()!;
            var decodedToken = _cryptoService.DecodeJose(token, certificate);

            /* edit response to mantain detail of original request */
            response.Content = new StringContent(decodedToken, Encoding.UTF8, "application/jwt");
        }
        return response;
    }
}