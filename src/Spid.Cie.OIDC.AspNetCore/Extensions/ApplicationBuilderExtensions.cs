using IdentityModel.AspNetCore.AccessTokenManagement;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Spid.Cie.OIDC.AspNetCore.Configuration;
using Spid.Cie.OIDC.AspNetCore.Events;
using Spid.Cie.OIDC.AspNetCore.Middlewares;
using Spid.Cie.OIDC.AspNetCore.Models;
using Spid.Cie.OIDC.AspNetCore.Services;
using Spid.Cie.OIDC.AspNetCore.Services.Defaults;
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
                options.CallbackPath = new PathString(SpidCieConst.CallbackPath);
                options.SignedOutCallbackPath = new PathString(SpidCieConst.SignedOutCallbackPath);
                options.RemoteSignOutPath = new PathString(SpidCieConst.RemoteSignOutPath);
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
        internalBuilder.Services.AddHttpClient(SpidCieConst.BackchannelClientName)
            .ConfigurePrimaryHttpMessageHandler(srv => (srv.GetService(typeof(CustomHttpClientHandler)) as CustomHttpClientHandler)!);

        internalBuilder.Services.TryAddScoped<SpidCieEvents>();
        internalBuilder.Services.TryAddScoped<IIdentityProvidersHandler, IdentityProvidersHandler>();
        internalBuilder.Services.TryAddScoped<IRelyingPartiesHandler, RelyingPartiesHandler>();
        internalBuilder.Services.TryAddScoped<ITrustChainManager, TrustChainManager>();
        internalBuilder.Services.TryAddScoped<IMetadataPolicyHandler, MetadataPolicyHandler>();

        internalBuilder.Services.TryAddScoped<IIdentityProvidersRetriever, DefaultIdentityProvidersRetriever>();
        internalBuilder.Services.TryAddScoped<IIdentityProviderSelector, DefaultIdentityProviderSelector>();
        internalBuilder.Services.TryAddScoped<IRelyingPartySelector, DefaultRelyingPartySelector>();
        internalBuilder.Services.TryAddScoped<IRelyingPartiesRetriever, DefaultRelyingPartiesRetriever>();
        internalBuilder.Services.TryAddScoped<ILogPersister, DefaultLogPersister>();

        internalBuilder.Services.TryAddScoped<IOptionsMonitor<OpenIdConnectOptions>, OpenIdConnectOptionsProvider>();
        internalBuilder.Services.TryAddScoped<IConfigurationManager<OpenIdConnectConfiguration>, ConfigurationManager>();

        internalBuilder.Services.AddAccessTokenManagement();
        internalBuilder.Services.AddHttpClient(AccessTokenManagementDefaults.BackChannelHttpClientName)
            .ConfigurePrimaryHttpMessageHandler(srv => (srv.GetService(typeof(CustomHttpClientHandler)) as CustomHttpClientHandler)!);

        internalBuilder.Services.AddScoped<ITokenClientConfigurationService, AssertionConfigurationService>();
        internalBuilder.Services.AddScoped<ICryptoService, CryptoService>();
        internalBuilder.Services.AddScoped<ITokenValidationParametersRetriever, TokenValidationParametersRetriever>();

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

    public static ISpidCieOIDCBuilder AddIdentityProvidersRetriever<T>(this ISpidCieOIDCBuilder builder)
        where T : class, IIdentityProvidersRetriever
    {
        builder.Services.AddScoped<IIdentityProvidersRetriever, T>();
        return builder;
    }

    public static IApplicationBuilder UseSpidCieOIDC(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RPOpenIdFederationMiddleware>();
    }
}
