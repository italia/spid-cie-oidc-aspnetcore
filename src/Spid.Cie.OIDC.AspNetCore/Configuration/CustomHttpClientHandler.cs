using Spid.Cie.OIDC.AspNetCore.Helpers;
using Spid.Cie.OIDC.AspNetCore.Services;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Spid.Cie.OIDC.AspNetCore.Configuration;

internal class CustomHttpClientHandler : HttpClientHandler
{
    private readonly IRelyingPartySelector _rpSelector;
    private readonly ICryptoService _cryptoService;

    public CustomHttpClientHandler(IRelyingPartySelector rpSelector,
        ICryptoService cryptoService)
    {
        _rpSelector = rpSelector;
        _cryptoService = cryptoService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = await base.SendAsync(request, cancellationToken);
        if (response.Content.Headers.ContentType?.MediaType == "application/jose")
        {
            return await DecodeJoseResponse(response);
        }
        return response;
    }

    public async Task<HttpResponseMessage> DecodeJoseResponse(HttpResponseMessage response)
    {
        var provider = await _rpSelector.GetSelectedRelyingParty();
        if (provider is not null)
        {
            var token = await response.Content.ReadAsStringAsync();
            var key = provider.OpenIdCoreJWKs?.Keys?.FirstOrDefault();
            if (key is not null)
            {
                (_, RSA privateKey) = _cryptoService.GetRSAKeys(key);

                var decodedToken = _cryptoService.DecodeJWT(_cryptoService.DecodeJose(token, privateKey));

                var httpResponse = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
                httpResponse.Content = new StringContent(decodedToken, Encoding.UTF8, "application/json");
                return httpResponse;
            }
        }
        return response;
    }
}