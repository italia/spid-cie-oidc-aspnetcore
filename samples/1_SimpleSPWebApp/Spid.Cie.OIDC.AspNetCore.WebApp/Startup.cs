using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Spid.Cie.OIDC.AspNetCore.Extensions;
using Spid.Cie.OIDC.AspNetCore.Models;
using System.Security.Cryptography.X509Certificates;

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
        var certificate = new X509Certificate2("wwwroot/certificates/ComuneVigata-SPID.pfx", "P@ssW0rd!");

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

                o.RequestRefreshToken = true;
                o.SpidOPs.Add("http://127.0.0.1:8000/oidc/op/");
                o.CieOPs.Add("http://127.0.0.1:8002/oidc/op/");
                o.RelyingParties.Add(new RelyingParty()
                {
                    ClientId = "http://127.0.0.1:5000/",
                    ClientName = "RP Test",
                    Contacts = new string[] { "info@rptest.it" },
                    Issuer = "http://127.0.0.1:5000/",
                    AuthorityHints = new string[] { "http://127.0.0.1:8000/" },
                    RedirectUris = new string[] { "http://127.0.0.1:5000/signin-spidcie" },
                    SecurityLevel = SecurityLevel.L2,
                    LongSessionsEnabled = false,
                    TrustMarks = new TrustMarkDefinition[] {
                            new TrustMarkDefinition() {
                                Id = "https://www.spid.gov.it/openid-federation/agreement/sp-public/",
                                Issuer = "http://127.0.0.1:8000/",
                                TrustMark = "eyJhbGciOiJSUzI1NiIsImtpZCI6IkZpZll4MDNibm9zRDhtNmdZUUlmTkhOUDljTV9TYW05VGM1bkxsb0lJcmMiLCJ0eXAiOiJ0cnVzdC1tYXJrK2p3dCJ9.eyJpc3MiOiJodHRwOi8vMTI3LjAuMC4xOjgwMDAvIiwic3ViIjoiaHR0cDovLzEyNy4wLjAuMTo1MDAwLyIsImlhdCI6MTY0Nzg1MjY1MSwiaWQiOiJodHRwczovL3d3dy5zcGlkLmdvdi5pdC9jZXJ0aWZpY2F0aW9uL3JwIiwibWFyayI6Imh0dHBzOi8vd3d3LmFnaWQuZ292Lml0L3RoZW1lcy9jdXN0b20vYWdpZC9sb2dvLnN2ZyIsInJlZiI6Imh0dHBzOi8vZG9jcy5pdGFsaWEuaXQvaXRhbGlhL3NwaWQvc3BpZC1yZWdvbGUtdGVjbmljaGUtb2lkYy9pdC9zdGFiaWxlL2luZGV4Lmh0bWwifQ.Mr6c4BhdjXWmytKzRA7SnnrI2vK40LXhPqwrOsBth1nySidvzdaBxSV-g7pyhVBnMhE4qZ24Jmche9D2p5EAdurpTf-szYfciHEQC3UbIkyRX7Mw3qLh91WhqW5oovpDGbq5AEa1TQReKhnrlawwngIt71Dj8MOY6PBrtTUP0_Vojo_AH9oXrc0IhXv0batJ-7acobdliIm3MDi6ZTr5xE-IMFuDAdtxCN-5idd930SupcPNC4c3Q685stDBiBnicKMRSCjHmVQdt1OVO9DwGn_ikueUGTjF4qGi33QBrujxvtc-wROcQAZazOiL5M12PBxl7cypD9ju0RIOdNNYvQ"
                            }
                        },
                    OpenIdCoreCertificates = new X509Certificate2[] { certificate },
                    OpenIdFederationCertificates = new X509Certificate2[] { certificate },
                    RequestedClaims = new ClaimTypes[] {
                            ClaimTypes.Name,
                            ClaimTypes.FamilyName,
                            ClaimTypes.Email,
                            ClaimTypes.FiscalNumber,
                            ClaimTypes.DateOfBirth,
                            ClaimTypes.PlaceOfBirth
                        }
                });
            });
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseDeveloperExceptionPage();
        app.UseHsts();
        app.UseStaticFiles();

        app.UseCookiePolicy(new CookiePolicyOptions
        {
            Secure = CookieSecurePolicy.Always
        });

        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseSpidCieOIDC();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllerRoute(
                                name: "default",
                                pattern: "{controller=Home}/{action=Index}/{id?}");
        });
    }
}
