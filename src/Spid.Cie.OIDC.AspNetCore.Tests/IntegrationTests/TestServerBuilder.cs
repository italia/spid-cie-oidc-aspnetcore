using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Spid.Cie.OIDC.AspNetCore.Configuration;
using Spid.Cie.OIDC.AspNetCore.Extensions;
using Spid.Cie.OIDC.AspNetCore.Services;
using Spid.Cie.OIDC.AspNetCore.Tests.Mocks;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using static Spid.Cie.OIDC.AspNetCore.Tests.Mocks.TestSettings;

namespace Spid.Cie.OIDC.AspNetCore.Tests.IntegrationTests;

internal class TestServerBuilder
{
    public static readonly string DefaultAuthority = @"http://127.0.0.1:8000/oidc/op/";
    public static readonly string TestHost = @"http://127.0.0.1:5000";
    public static readonly string Challenge = "/challenge";
    public static readonly string ChallengeWithProvider = "/challenge?provider=http://127.0.0.1:8000/oidc/op/";
    public static readonly string ChallengeWithOutContext = "/challengeWithOutContext";
    public static readonly string ChallengeWithProperties = "/challengeWithProperties";
    public static readonly string Signin = "/signin";
    public static readonly string Signout = "/signout";

    public static OpenIdConnectOptions CreateOpenIdConnectOptions() =>
        new OpenIdConnectOptions
        {
            Authority = DefaultAuthority,
            ClientId = Guid.NewGuid().ToString(),
            Configuration = CreateDefaultOpenIdConnectConfiguration()
        };

    public static OpenIdConnectOptions CreateOpenIdConnectOptions(Action<OpenIdConnectOptions> update)
    {
        var options = CreateOpenIdConnectOptions();
        update?.Invoke(options);
        return options;
    }

    public static OpenIdConnectConfiguration CreateDefaultOpenIdConnectConfiguration() =>
        new OpenIdConnectConfiguration()
        {
            AuthorizationEndpoint = DefaultAuthority + "/oauth2/authorize",
            EndSessionEndpoint = DefaultAuthority + "/oauth2/endsessionendpoint",
            TokenEndpoint = DefaultAuthority + "/oauth2/token"
        };

    public static IConfigurationManager<OpenIdConnectConfiguration> CreateDefaultOpenIdConnectConfigurationManager() =>
        new StaticConfigurationManager<OpenIdConnectConfiguration>(CreateDefaultOpenIdConnectConfiguration());

    public static TestServer CreateServer(Action<SpidCieOptions> options)
    {
        return CreateServer(options, handler: null, properties: null);
    }

    public static TestServer CreateServer(
        Action<SpidCieOptions> options,
        Func<HttpContext, Task>? handler,
        AuthenticationProperties? properties)
    {
        var host = new HostBuilder()
            .ConfigureWebHost(builder =>
                builder.UseTestServer()
                    .Configure(app =>
                    {
                        app.UseSpidCieOIDC();
                        app.UseAuthentication();
                        app.Use(async (context, next) =>
                        {
                            var req = context.Request;
                            var res = context.Response;

                            if (req.Path == new PathString(Challenge))
                            {
                                await context.ChallengeAsync(SpidCieConst.AuthenticationScheme);
                            }
                            else if (req.Path == new PathString(ChallengeWithProperties))
                            {
                                await context.ChallengeAsync(SpidCieConst.AuthenticationScheme, properties);
                            }
                            else if (req.Path == new PathString(ChallengeWithOutContext))
                            {
                                res.StatusCode = 401;
                            }
                            else if (req.Path == new PathString(Signin))
                            {
                                await context.SignInAsync(SpidCieConst.AuthenticationScheme, new ClaimsPrincipal());
                            }
                            else if (req.Path == new PathString(Signout))
                            {
                                await context.SignOutAsync(SpidCieConst.AuthenticationScheme);
                            }
                            else if (req.Path == new PathString("/signout_with_specific_redirect_uri"))
                            {
                                await context.SignOutAsync(
                                    SpidCieConst.AuthenticationScheme,
                                    new AuthenticationProperties() { RedirectUri = "http://www.example.com/specific_redirect_uri" });
                            }
                            else if (handler != null)
                            {
                                await handler(context);
                            }
                            else
                            {
                                await next(context);
                            }
                        });
                    })
                    .ConfigureServices(services =>
                    {
                        SpidCieConst.TrustChainExpirationGracePeriod = TimeSpan.FromDays(3650);
                        services.AddAuthentication(o =>
                        {
                            o.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                            o.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                            o.DefaultChallengeScheme = SpidCieConst.AuthenticationScheme;
                        })
                        .AddCookie()
                        .AddSpidCieOIDC(options);
                        services.AddScoped<MockBackchannel>();
                        services.AddHttpClient("SpidCieBackchannel")
                            .ConfigurePrimaryHttpMessageHandler(srv => (srv.GetService(typeof(MockBackchannel)) as MockBackchannel)!);
                        services.AddScoped<ICryptoService, MockCryptoService>();
                        services.AddScoped<ITokenValidationParametersRetriever, MockTokenValidationParametersRetriever>();
                        services.AddScoped<IIdentityProviderSelector>(srv => new MockIdentityProviderSelector(false));
                        services.AddScoped<IIdentityProvidersHandler>(srv => new MockIdentityProvidersHandler(false));
                        services.TryAddEnumerable(ServiceDescriptor.Singleton<IPostConfigureOptions<OpenIdConnectOptions>, MockOpenIdConnectPostConfigureOptions>());
                    }))
            .Build();

        host.Start();
        return host.GetTestServer();
    }
}