using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Spid.Cie.OIDC.AspNetCore.Configuration;
using Spid.Cie.OIDC.AspNetCore.Events;
using Spid.Cie.OIDC.AspNetCore.Logging;
using Spid.Cie.OIDC.AspNetCore.Models;
using Spid.Cie.OIDC.AspNetCore.OpenIdFederation;
using Spid.Cie.OIDC.AspNetCore.Services;
using System;

namespace Spid.Cie.OIDC.AspNetCore.Extensions;

public static class ApplicationBuilderExtensions
{
    private static ISpidCieOIDCBuilder AddSpidCieOIDCBuilder(this IServiceCollection services)
    {
        return new SpidCieOIDCBuilder(services);
    }

    public static ISpidCieOIDCBuilder AddSpidCieOIDC(this AuthenticationBuilder builder)
        => builder.AddSpidCieOIDC(o => { });

    public static ISpidCieOIDCBuilder AddSpidCieOIDC(this AuthenticationBuilder builder, Action<SpidCieOptions> configureOptions)
    {
        var internalBuilder = builder.Services.AddSpidCieOIDCBuilder();

        internalBuilder.Services.Configure(configureOptions);
        internalBuilder.Services.AddHttpContextAccessor();
        internalBuilder.Services.TryAddScoped<SpidCieEvents>();
        internalBuilder.Services.TryAddScoped<ILogPersister, DefaultLogPersister>();
        internalBuilder.Services.TryAddScoped<IIdentityProvidersRetriever, IdentityProvidersRetriever>();
        internalBuilder.Services.TryAddScoped<IIdentityProviderSelector, IdentityProviderSelector>();
        internalBuilder.Services.TryAddScoped<IRelyingPartiesRetriever, DefaultRelyingPartiesRetriever>();
        internalBuilder.Services.TryAddScoped<IRelyingPartySelector, DefaultRelyingPartySelector>();
        internalBuilder.Services.AddSingleton<IOptionsMonitor<OpenIdConnectOptions>, OpenIdConnectOptionsProvider>();
        internalBuilder.Services.AddSingleton<IConfigureOptions<OpenIdConnectOptions>, OpenIdConnectOptionsInitializer>();

        builder.AddOpenIdConnect(SpidCieDefaults.AuthenticationScheme, options =>
        {
            options.ResponseType = SpidCieDefaults.ResponseType;
            options.Scope.Add(SpidCieDefaults.OpenIdScope);
            options.Prompt = SpidCieDefaults.Prompt;
            options.UsePkce = true;
            options.GetClaimsFromUserInfoEndpoint = true;
            options.Events.OnRedirectToIdentityProvider = (context) => context.HttpContext.RequestServices.GetRequiredService<SpidCieEvents>().OnRedirectToIdentityProvider(context);
        });

        return internalBuilder;
    }

    public static ISpidCieOIDCBuilder AddLogPersister<T>(this ISpidCieOIDCBuilder builder)
        where T : class, ILogPersister
    {
        builder.Services.AddScoped<ILogPersister, T>();
        return builder;
    }

    public static ISpidCieOIDCBuilder AddRelyingPartySelector<T>(this ISpidCieOIDCBuilder builder)
        where T : class, IRelyingPartySelector
    {
        builder.Services.AddScoped<IRelyingPartySelector, T>();
        return builder;
    }

    public static ISpidCieOIDCBuilder AddRelyingPartiesRetriever<T>(this ISpidCieOIDCBuilder builder)
        where T : class, IRelyingPartiesRetriever
    {
        builder.Services.AddScoped<IRelyingPartiesRetriever, T>();
        return builder;
    }

    public static IApplicationBuilder UseSpidCieOIDC(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<OpenIdFederationMiddleware>();
    }
}
