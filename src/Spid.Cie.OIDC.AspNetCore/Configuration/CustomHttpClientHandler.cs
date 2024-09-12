using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Spid.Cie.OIDC.AspNetCore.Helpers;
using Spid.Cie.OIDC.AspNetCore.Services;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Spid.Cie.OIDC.AspNetCore.Configuration;

class CustomHttpClientHandler : HttpClientHandler
{
    readonly ILogPersister _logPersister;
    readonly ICryptoService _cryptoService;
    readonly IAggregatorsHandler _aggHandler;
    readonly IRelyingPartySelector _rpSelector;
    readonly IHttpContextAccessor _httpContextAccessor;

    public CustomHttpClientHandler(IRelyingPartySelector rpSelector, ILogPersister logPersister, ICryptoService cryptoService, IAggregatorsHandler aggHandler,
                                                IHttpContextAccessor httpContextAccessor)
    {
        _aggHandler = aggHandler;
        _rpSelector = rpSelector;
        _logPersister = logPersister;
        _cryptoService = cryptoService;
        _httpContextAccessor = httpContextAccessor;
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

            if (provider == default)
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

                provider = aggregators.SelectMany(a => a.RelyingParties)
                                .OrderByDescending(r => r.Id.Length)
                                .FirstOrDefault(r => uri.StartsWith(r.Id.EnsureTrailingSlash(), StringComparison.OrdinalIgnoreCase));
            }

            Throw<Exception>.If(provider is null, "No currently selected RelyingParty was found");
            Throw<Exception>.If(provider!.OpenIdCoreCertificates is null || provider!.OpenIdCoreCertificates.Count() == 0,
                "No OpenIdCore Certificates were found in the currently selected RelyingParty");

            var openIdCoreCertificate = provider!.OpenIdCoreCertificates!.FirstOrDefault(occ => occ.KeyUsage == Enums.KeyUsageTypes.Encryption)!;
            var decodedToken = _cryptoService.DecodeJose(token, openIdCoreCertificate.Certificate!);

            /* edit response to mantain detail of original request */
            response.Content = new StringContent(decodedToken, Encoding.UTF8, "application/jwt");
        }

        return response;
    }
}