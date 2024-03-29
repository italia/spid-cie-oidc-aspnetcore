﻿using Microsoft.Extensions.DependencyInjection;
using Spid.Cie.OIDC.AspNetCore.Extensions;
using Spid.Cie.OIDC.AspNetCore.Services.Defaults;
using System;
using Xunit;

namespace Spid.Cie.OIDC.AspNetCore.Tests;

public class ApplicationBuilderExtensionsTests
{
    [Fact]
    public void SpidCieOIDCBuilder()
    {
        IServiceCollection service = new ServiceCollection();
        Assert.NotNull(new SpidCieOIDCBuilder(service));
        Assert.NotNull(new SpidCieOIDCBuilder(service).Services);
    }

    [Fact]
    public void SpidCieOIDCBuilderThrows()
    {
        Assert.ThrowsAny<Exception>(() => new SpidCieOIDCBuilder(null));
    }

    [Fact]
    public void DefaultRelyingPartiesRetriever()
    {
        IServiceCollection service = new ServiceCollection();
        service.AddAuthentication()
            .AddSpidCieOIDC()
            .AddRelyingPartiesRetriever<DefaultRelyingPartiesRetriever>();
    }

    [Fact]
    public void DefaultIdentityProvidersRetriever()
    {
        IServiceCollection service = new ServiceCollection();
        service.AddAuthentication()
            .AddSpidCieOIDC()
            .AddIdentityProvidersRetriever<DefaultIdentityProvidersRetriever>();
    }

    [Fact]
    public void ILogPersister()
    {
        IServiceCollection service = new ServiceCollection();
        service.AddAuthentication()
            .AddSpidCieOIDC()
            .AddLogPersister<DefaultLogPersister>();
    }
}
