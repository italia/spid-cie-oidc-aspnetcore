//using Microsoft.Extensions.Logging;
//using Moq;
//using Moq.Protected;
//using Spid.Cie.OIDC.AspNetCore.Services;
//using System;
//using System.Linq;
//using System.Net;
//using System.Net.Http;
//using System.Threading;
//using System.Threading.Tasks;
//using Xunit;

//namespace Spid.Cie.OIDC.AspNetCore.Tests;

//public class IdentityProviderRetrieverTests
//{
//    private readonly IIdentityProvidersRetriever _retriever;

//    public IdentityProviderRetrieverTests()
//    {
//        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
//        handlerMock
//           .Protected()
//           .Setup<Task<HttpResponseMessage>>(
//              "SendAsync",
//              ItExpr.IsAny<HttpRequestMessage>(),
//              ItExpr.IsAny<CancellationToken>()
//           )
//           .ReturnsAsync(new HttpResponseMessage()
//           {
//               StatusCode = HttpStatusCode.OK,
//               Content = new StringContent("[ \"http://127.0.0.1/\" ]"),
//           })
//           .Verifiable();
//        var httpClient = new HttpClient(handlerMock.Object)
//        {
//            BaseAddress = new Uri("http://test.com/"),
//        };

//        this._retriever = new IdentityProvidersRetriever(new Mocks.MockOptionsMonitorSpidCieOptions(),
//            httpClient,
//            new Mocks.MockTrustChainManager(),
//            Mock.Of<ILogger<IdentityProvidersRetriever>>());
//    }

//    [Fact]
//    public async Task TestGetIdentityProviders()
//    {
//        var idp = await _retriever.GetIdentityProviders();
//        Assert.NotNull(idp);
//        Assert.True(idp.Count() != 0);
//    }
//}
