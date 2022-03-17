﻿using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
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
        builder.AddRemoteScheme<OpenIdConnectOptions, SpidCieHandler>(SpidCieConst.AuthenticationScheme, SpidCieConst.AuthenticationScheme,
            options =>
            {
                options.ClientId = SpidCieConst.DummyUrl;
                options.MetadataAddress = SpidCieConst.DummyUrl;
                options.SaveTokens = true;
                options.ResponseMode = string.Empty;
                options.ResponseType = SpidCieConst.ResponseType;
                options.Prompt = SpidCieConst.Prompt;
                options.UsePkce = true;
                options.GetClaimsFromUserInfoEndpoint = true;
                options.AuthenticationMethod = OpenIdConnectRedirectBehavior.RedirectGet;

                options.Scope.Clear();
                options.Scope.Add(SpidCieConst.OpenIdScope);

                options.Events.OnMessageReceived = context => context.HttpContext.RequestServices.GetRequiredService<SpidCieEvents>().MessageReceived(context);
                options.Events.OnAuthorizationCodeReceived = context => context.HttpContext.RequestServices.GetRequiredService<SpidCieEvents>().AuthorizationCodeReceived(context);

                options.ClaimActions.MapAll();
                options.ClaimActions.Remove(System.Security.Claims.ClaimTypes.Name);
                options.ClaimActions.Remove(System.Security.Claims.ClaimTypes.Role);
            });

        var internalBuilder = builder.Services.AddSpidCieOIDCBuilder();

        internalBuilder.Services.Configure(configureOptions);
        internalBuilder.Services.AddHttpContextAccessor();
        internalBuilder.Services.TryAddScoped<CustomHttpClientHandler>();
        internalBuilder.Services.AddHttpClient("SpidCieBackchannel")
            .ConfigurePrimaryHttpMessageHandler(srv => (srv.GetService(typeof(CustomHttpClientHandler)) as CustomHttpClientHandler)!);

        internalBuilder.Services.TryAddScoped<SpidCieEvents>();
        internalBuilder.Services.TryAddScoped<ILogPersister, DefaultLogPersister>();
        internalBuilder.Services.TryAddScoped<IIdentityProvidersRetriever, IdentityProvidersRetriever>();
        internalBuilder.Services.TryAddScoped<IIdentityProviderSelector, DefaultIdentityProviderSelector>();
        internalBuilder.Services.TryAddScoped<IRelyingPartiesRetriever, DefaultRelyingPartiesRetriever>();
        internalBuilder.Services.TryAddScoped<IRelyingPartySelector, DefaultRelyingPartySelector>();
        internalBuilder.Services.TryAddScoped<ITrustChainManager, TrustChainManager>();

        internalBuilder.Services.TryAddScoped<IOptionsMonitor<OpenIdConnectOptions>, OpenIdConnectOptionsProvider>();
        internalBuilder.Services.TryAddScoped<IConfigurationManager<OpenIdConnectConfiguration>, ConfigurationManager>();

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