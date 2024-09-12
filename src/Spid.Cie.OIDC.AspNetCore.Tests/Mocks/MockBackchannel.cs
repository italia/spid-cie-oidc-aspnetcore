using Microsoft.AspNetCore.Http;
using Spid.Cie.OIDC.AspNetCore.Configuration;
using Spid.Cie.OIDC.AspNetCore.Services;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Spid.Cie.OIDC.AspNetCore.Tests.Mocks;

internal partial class TestSettings
{
    internal class MockBackchannel : CustomHttpClientHandler
    {
        public MockBackchannel(IRelyingPartySelector rpSelector, ILogPersister logPersister,
            ICryptoService cryptoService, IAggregatorsHandler aggregatorsHandler,
            IHttpContextAccessor contextAccessor)
            : base(rpSelector, logPersister, cryptoService, aggregatorsHandler, contextAccessor)
        {
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.RequestUri.AbsoluteUri.Equals("http://127.0.0.1:8000/list/?type=openid_provider"))
            {
                return await ReturnResource("oplist.json", "application/json");
            }
            if (request.RequestUri.AbsoluteUri.Equals("http://127.0.0.1:8000/oidc/op/.well-known/openid-federation"))
            {
                return await ReturnResource("jwtOP.json", "application/jwt");
            }
            if (request.RequestUri.AbsoluteUri.Equals("http://127.0.0.1:8002/oidc/op/.well-known/openid-federation"))
            {
                return await ReturnResource("jwtOP.json", "application/jwt");
            }
            if (request.RequestUri.AbsoluteUri.Equals("http://127.0.0.1:8000/.well-known/openid-federation"))
            {
                return await ReturnResource("jwtTA.json", "application/jwt");
            }
            if (request.RequestUri.AbsoluteUri.Equals("http://127.0.0.1:8000/fetch/?sub=http://127.0.0.1:8000/oidc/op/"))
            {
                return await ReturnResource("jwtES.json", "application/jwt");
            }
            if (request.RequestUri.AbsoluteUri.Equals("http://127.0.0.1:8000/fetch/?sub=http://127.0.0.1:8002/oidc/op/"))
            {
                return await ReturnResource("jwtES.json", "application/jwt");
            }

            if (request.RequestUri.AbsoluteUri.Equals("http://127.0.0.1:8000/oidc/op/authorization"))
            {
                return await ReturnResource("jwtES.json", "application/jwt");
            }
            if (request.RequestUri.AbsoluteUri.Equals("http://127.0.0.1:8000/oidc/op/token/"))
            {
                return await ReturnResource("tokenResponse.json", "application/json");
            }
            if (request.RequestUri.AbsoluteUri.Equals("http://127.0.0.1:8000/oidc/op/userinfo/"))
            {
                return await base.DecodeJoseResponse(await ReturnResource("userInfoResponse.jose", "application/jose"));
            }
            if (request.RequestUri.AbsoluteUri.Equals("http://127.0.0.1:8000/oidc/op/revocation/"))
            {
                return await ReturnResource("revocationResponse.json", "application/json");
            }

            throw new NotImplementedException();
        }

        private async Task<HttpResponseMessage> ReturnResource(string resource, string contentType)
        {
            var resourceName = "Spid.Cie.OIDC.AspNetCore.Tests.IntegrationTests." + resource;
            using (var stream = typeof(MockBackchannel).Assembly.GetManifestResourceStream(resourceName))
            using (var reader = new StreamReader(stream))
            {
                var body = await reader.ReadToEndAsync();
                var content = new StringContent(body, Encoding.UTF8, contentType);
                return new HttpResponseMessage()
                {
                    Content = content,
                };
            }
        }
    }
}
