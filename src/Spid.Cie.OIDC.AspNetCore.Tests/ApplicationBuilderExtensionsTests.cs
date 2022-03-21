using Microsoft.Extensions.DependencyInjection;
using Spid.Cie.OIDC.AspNetCore.Extensions;
using Spid.Cie.OIDC.AspNetCore.Services;
using Spid.Cie.OIDC.AspNetCore.Tests.Mocks;
using Xunit;

namespace Spid.Cie.OIDC.AspNetCore.Tests;

public class ApplicationBuilderExtensionsTests
{
    [Fact]
    public void DefaultRelyingPartiesRetriever()
    {
        IServiceCollection service = new ServiceCollection();
        service.AddAuthentication()
            .AddSpidCieOIDC()
            .AddRelyingPartiesRetriever<DefaultRelyingPartiesRetriever>();
    }

    [Fact]
    public void DefaultRelyingPartySelector()
    {
        IServiceCollection service = new ServiceCollection();
        service.AddAuthentication()
            .AddSpidCieOIDC()
            .AddRelyingPartySelector<DefaultRelyingPartySelector>();
    }

    [Fact]
    public void ILogPersister()
    {
        IServiceCollection service = new ServiceCollection();
        service.AddAuthentication()
            .AddSpidCieOIDC()
            .AddLogPersister<MockLogPersister>();
    }
}
