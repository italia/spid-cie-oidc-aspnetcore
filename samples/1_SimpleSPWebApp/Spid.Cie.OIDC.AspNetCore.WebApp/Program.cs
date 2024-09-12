using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.IdentityModel.Logging;
using Spid.Cie.OIDC.AspNetCore;
using Spid.Cie.OIDC.AspNetCore.Extensions;

IdentityModelEventSource.ShowPII = true;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication(o =>
    {
        o.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        o.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        o.DefaultChallengeScheme = SpidCieConst.AuthenticationScheme;
    })
    .AddCookie()
    .AddSpidCieOIDC(o =>
    {
        o.LoadFromConfiguration(builder.Configuration);
    });

var app = builder.Build();

app.UseExceptionHandler("/Home/Error");
app.UseStaticFiles();

app.UseCookiePolicy(new CookiePolicyOptions
{
    Secure = CookieSecurePolicy.None
});

app.UseSpidCieOIDC();

app.UseCookiePolicy(
    new CookiePolicyOptions
    {
        Secure = CookieSecurePolicy.None,
        MinimumSameSitePolicy = SameSiteMode.Lax
    });

app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");

app.UseAuthentication();
app.UseAuthorization();

app.Run();