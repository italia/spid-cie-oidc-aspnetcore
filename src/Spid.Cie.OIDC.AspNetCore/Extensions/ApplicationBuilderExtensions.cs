using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
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
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IPostConfigureOptions<OpenIdConnectOptions>, OpenIdConnectPostConfigureOptions>());
        builder.AddRemoteScheme<OpenIdConnectOptions, SpidCieHandler>(SpidCieDefaults.AuthenticationScheme, SpidCieDefaults.AuthenticationScheme,
            options =>
            {
                options.ClientId = SpidCieDefaults.DummyUrl;
                options.MetadataAddress = SpidCieDefaults.DummyUrl;
                options.SaveTokens = true;
                options.ResponseMode = null;
                options.ResponseType = SpidCieDefaults.ResponseType;
                options.Scope.Clear();
                options.Scope.Add(SpidCieDefaults.OpenIdScope);
                options.Scope.Add(SpidCieDefaults.OfflineScope);
                options.Prompt = SpidCieDefaults.Prompt;
                options.UsePkce = true;
                options.GetClaimsFromUserInfoEndpoint = true;
                options.Events.OnMessageReceived = context => context.HttpContext.RequestServices.GetRequiredService<SpidCieEvents>().MessageReceived(context);
                options.Events.OnAuthorizationCodeReceived = context => context.HttpContext.RequestServices.GetRequiredService<SpidCieEvents>().AuthorizationCodeReceived(context);

                options.ClaimActions.MapAll();
                options.ClaimActions.Remove(System.Security.Claims.ClaimTypes.Name);
                options.ClaimActions.Remove(System.Security.Claims.ClaimTypes.Role);
            });

        var internalBuilder = builder.Services.AddSpidCieOIDCBuilder();

        internalBuilder.Services.Configure(configureOptions);
        internalBuilder.Services.AddHttpContextAccessor();
        internalBuilder.Services.AddHttpClient();
        internalBuilder.Services.TryAdd(ServiceDescriptor.Singleton<IActionContextAccessor, ActionContextAccessor>());
        internalBuilder.Services.TryAddScoped(delegate (IServiceProvider factory)
        {
            ActionContext actionContext = factory.GetService<IActionContextAccessor>()!.ActionContext;
            return factory.GetService<IUrlHelperFactory>()!.GetUrlHelper(actionContext);
        });

        internalBuilder.Services.TryAddScoped<SpidCieEvents>();
        internalBuilder.Services.TryAddScoped<ILogPersister, DefaultLogPersister>();
        internalBuilder.Services.TryAddScoped<IIdentityProvidersRetriever, IdentityProvidersRetriever>();
        internalBuilder.Services.TryAddScoped<IIdentityProviderSelector, DefaultIdentityProviderSelector>();
        internalBuilder.Services.TryAddScoped<IRelyingPartiesRetriever, DefaultRelyingPartiesRetriever>();
        internalBuilder.Services.TryAddScoped<IRelyingPartySelector, DefaultRelyingPartySelector>();

        internalBuilder.Services.TryAddScoped<IOptionsMonitor<OpenIdConnectOptions>, OpenIdConnectOptionsProvider>();
        internalBuilder.Services.TryAddScoped<IConfigurationManager<OpenIdConnectConfiguration>, ConfigurationManager>();

        internalBuilder.Services.TryAddScoped<CustomHttpClientHandler>();

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
        return builder.UseMiddleware<RPOpenIdFederationMiddleware>()
            .UseMiddleware<JWKGeneratorMiddleware>();
    }
}
