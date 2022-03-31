using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Spid.Cie.OIDC.AspNetCore.Extensions;
using Spid.Cie.OIDC.AspNetCore.Models;

namespace Spid.Cie.OIDC.AspNetCore.WebApp;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }


    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllersWithViews();
        services
            .AddAuthentication(o =>
            {
                o.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                o.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                o.DefaultChallengeScheme = SpidCieConst.AuthenticationScheme;
            })
            .AddCookie()
            .AddSpidCieOIDC(o =>
            {
                o.LoadFromConfiguration(Configuration);
            });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseStaticFiles();

        app.UseCookiePolicy(new CookiePolicyOptions
        {
            Secure = CookieSecurePolicy.None
        });

        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseSpidCieOIDC();

        app.UseCookiePolicy(
            new CookiePolicyOptions
            {
                Secure = CookieSecurePolicy.None,
                MinimumSameSitePolicy = SameSiteMode.Lax
            });

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllerRoute(
                                name: "default",
                                pattern: "{controller=Home}/{action=Index}/{id?}");
        });
    }
}
