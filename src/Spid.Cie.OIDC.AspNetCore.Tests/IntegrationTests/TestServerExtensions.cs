using Microsoft.AspNetCore.TestHost;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Spid.Cie.OIDC.AspNetCore.Tests.IntegrationTests;

internal static class TestServerExtensions
{
    public static Task<TestTransaction> SendAsync(this TestServer server, string url)
    {
        return server.SendAsync(url, cookieHeader: null);
    }

    public static Task<TestTransaction> SendAsync(this TestServer server, string url, string cookieHeader = null, string[] cookieHeaders = null)
    {
        return server.SendAsync(new HttpRequestMessage(HttpMethod.Get, url), cookieHeader, cookieHeaders);
    }


    public static async Task<TestTransaction> SendAsync(this TestServer server, HttpRequestMessage request, string cookieHeader, string[] cookieHeaders = null)
    {
        if (!string.IsNullOrEmpty(cookieHeader))
        {
            request.Headers.Add("Cookie", cookieHeader);
        }

        if (cookieHeaders != null)
        {
            foreach (var cookie in cookieHeaders)
                request.Headers.Add("Cookie", cookie);
        }


        var transaction = new TestTransaction
        {
            Request = request,
            Response = await server.CreateClient().SendAsync(request),
        };

        if (transaction.Response.Headers.Contains("Set-Cookie"))
        {
            transaction.SetCookie = transaction.Response.Headers.GetValues("Set-Cookie").ToList();
        }

        transaction.ResponseText = await transaction.Response.Content.ReadAsStringAsync();
        if (transaction.Response.Content != null &&
            transaction.Response.Content.Headers.ContentType != null &&
            transaction.Response.Content.Headers.ContentType.MediaType == "text/xml")
        {
            transaction.ResponseElement = XElement.Parse(transaction.ResponseText);
        }

        return transaction;
    }
}