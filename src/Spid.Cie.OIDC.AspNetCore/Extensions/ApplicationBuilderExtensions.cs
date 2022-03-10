using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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
        internalBuilder.Services.TryAddScoped<SpidEvents>();
        internalBuilder.Services.TryAddScoped<ILogPersister, DefaultLogPersister>();
        internalBuilder.Services.TryAddScoped<IIdentityProvidersRetriever, IdentityProvidersRetriever>();
        internalBuilder.Services.TryAddScoped<IIdentityProviderSelector, IdentityProviderSelector>();
        internalBuilder.Services.TryAddScoped<IRelyingPartiesRetriever, DefaultRelyingPartiesRetriever>();
        internalBuilder.Services.TryAddScoped<IRelyingPartySelector, DefaultRelyingPartySelector>();

        builder.AddOpenIdConnect(Defaults.AuthenticationScheme, options =>
        {
            options.ResponseType = Defaults.ResponseType;
            options.Scope.Add(Defaults.OpenIdScope);
            options.Prompt = Defaults.Prompt;
            options.UsePkce = true;
            options.GetClaimsFromUserInfoEndpoint = true;
            options.Events.OnRedirectToIdentityProvider = (context) => context.HttpContext.RequestServices.GetRequiredService<SpidEvents>().OnRedirectToIdentityProvider(context);
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
