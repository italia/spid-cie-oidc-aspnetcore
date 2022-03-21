using Microsoft.Extensions.Logging;
using Moq;
using Spid.Cie.OIDC.AspNetCore.Logging;
using Xunit;

namespace Spid.Cie.OIDC.AspNetCore.Tests;

public class LogPersisterTests
{
    [Fact]
    public async void LogRequest()
    {
        var logPersister = new DefaultLogPersister(Mock.Of<ILogger<DefaultLogPersister>>());
        await logPersister.LogRequest(new System.Uri("http://127.0.0.1"), string.Empty);
    }

    [Fact]
    public async void LogResponse()
    {
        var logPersister = new DefaultLogPersister(Mock.Of<ILogger<DefaultLogPersister>>());
        await logPersister.LogResponse(new System.Uri("http://127.0.0.1"), System.Net.HttpStatusCode.OK, string.Empty);
    }

    [Fact]
    public async void LogGetEntityConfiguration()
    {
        var logPersister = new DefaultLogPersister(Mock.Of<ILogger<DefaultLogPersister>>());
        await logPersister.LogGetEntityConfiguration("http://127.0.0.1", string.Empty);
    }

    [Fact]
    public async void LogGetEntityStatement()
    {
        var logPersister = new DefaultLogPersister(Mock.Of<ILogger<DefaultLogPersister>>());
        await logPersister.LogGetEntityStatement("http://127.0.0.1", string.Empty);
    }
}
