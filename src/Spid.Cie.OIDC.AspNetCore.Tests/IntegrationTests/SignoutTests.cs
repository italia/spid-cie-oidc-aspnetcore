using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Xunit;

namespace Spid.Cie.OIDC.AspNetCore.Tests.IntegrationTests;

public class SignoutTests
{
    private static readonly string SignoutEndpoint = TestServerBuilder.TestHost + TestServerBuilder.Signout;
    private static readonly string ChallengeEndpoint = TestServerBuilder.TestHost + TestServerBuilder.ChallengeWithProvider;

    [Fact]
    public async Task RegularSignoutRequest()
    {
        var settings = new TestSettings();

        var server = settings.CreateTestServer();
        var transaction = await server.SendAsync(ChallengeEndpoint);

        var res = transaction.Response;
        Assert.Equal(HttpStatusCode.Redirect, res.StatusCode);
        Assert.NotNull(res.Headers.Location);

        var location = res.Headers.Location;
        var cookies = SetCookieHeaderValue.ParseList(transaction.SetCookie);

        var queryString = QueryHelpers.ParseQuery(location.OriginalString);

        var response = await GetAsync(server, $"{TestServerBuilder.TestHost}{SpidCieConst.CallbackPath}?state={queryString["state"]}&iss={UrlEncoder.Default.Encode("http://127.0.0.1:8000/oidc/op/")}&code=test_code", cookies);
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.NotNull(res.Headers.Location);


        var cookieValues = response.Headers.GetValues("Set-Cookie").ToArray();
        transaction = await server.SendAsync(SignoutEndpoint, null, cookieValues);

        res = transaction.Response;
        Assert.Equal(HttpStatusCode.Redirect, res.StatusCode);
        Assert.NotNull(res.Headers.Location);
    }

    private Task<HttpResponseMessage> GetAsync(TestServer server, string path, IEnumerable<SetCookieHeaderValue> cookies)
    {
        var client = server.CreateClient();
        foreach (var cookie in cookies)
        {
            client.DefaultRequestHeaders.Add("Cookie", new CookieHeaderValue(cookie.Name, cookie.Value).ToString());
        }
        return client.GetAsync(path);
    }
}
