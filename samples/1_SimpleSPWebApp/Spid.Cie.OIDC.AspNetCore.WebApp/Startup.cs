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
                    ClientId = "http://127.0.0.1:5000/rp1",
                    ClientName = "RP Test",
                    Contacts = new string[] { "info@rptest.it" },
                    Issuer = "http://127.0.0.1:5000/rp1",
                    AuthorityHints = new string[] { "http://127.0.0.1:8000/" },
                    RedirectUris = new string[] { "http://127.0.0.1:5000/rp1/signin-spidcie" },
                    SecurityLevel = SecurityLevel.L2,
                    LongSessionsEnabled = false,
                    TrustMarks = new TrustMarkDefinition[] {
                            new TrustMarkDefinition() {
                                Id = "https://www.spid.gov.it/openid-federation/agreement/sp-public/",
                                Issuer = "http://127.0.0.1:8000/",
                                TrustMark = "eyJhbGciOiJSUzI1NiIsImtpZCI6ImRCNjdnTDdjazNURmlJQWY3TjZfN1NIdnFrME1EWU1FUWNvR0dsa1VBQXciLCJ0eXAiOiJ0cnVzdC1tYXJrK2p3dCJ9.eyJpc3MiOiJodHRwOi8vMTI3LjAuMC4xOjgwMDAvb2lkYy9vcC8iLCJzdWIiOiJodHRwOi8vMTI3LjAuMC4xOjUwMDAvIiwiaWF0IjoxNjQ4NTYwMjgzLCJpZCI6Imh0dHBzOi8vd3d3LnNwaWQuZ292Lml0L2NlcnRpZmljYXRpb24vcnAiLCJtYXJrIjoiaHR0cHM6Ly93d3cuYWdpZC5nb3YuaXQvdGhlbWVzL2N1c3RvbS9hZ2lkL2xvZ28uc3ZnIiwicmVmIjoiaHR0cHM6Ly9kb2NzLml0YWxpYS5pdC9pdGFsaWEvc3BpZC9zcGlkLXJlZ29sZS10ZWNuaWNoZS1vaWRjL2l0L3N0YWJpbGUvaW5kZXguaHRtbCJ9.M6T42JXb9wHBhwy2cueHEFoMNcaHQZKvMTMR3aavZVBvW14hps_IZ_MT3yqA5wTEZTgAC_-M8G33wjpTMw26ITXgOW6rMUqWHWj7639BfbqnGkoZdMuMxo96nSOIaxXElvfPRZu6wQ9LOrXe_kyR3eo6p8iZLKbnp1e1D5VTr_dSEYQsaTlVmiT6I2SyaiWtpXCD1DWZxNw2YKTie0lEmDCMO4WJo3kfr_ak9kvMryF8-5crecOs6o33DYGR5zSzJ3JQYAz2huLKZ_y7nzmvkEjDQNjtg1R2cusNHvxLt8Su3T0hUbT_Vl5-b_VvBqo2yUTc7Z7WOJPwzId8rATujA"
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
                o.RelyingParties.Add(new RelyingParty()
                {
                    ClientId = "http://127.0.0.1:5000/rp2",
                    ClientName = "RP Test",
                    Contacts = new string[] { "info@rptest.it" },
                    Issuer = "http://127.0.0.1:5000/rp2",
                    AuthorityHints = new string[] { "http://127.0.0.1:8000/" },
                    RedirectUris = new string[] { "http://127.0.0.1:5000/rp2/signin-spidcie" },
                    SecurityLevel = SecurityLevel.L2,
                    LongSessionsEnabled = false,
                    TrustMarks = new TrustMarkDefinition[] {
                            new TrustMarkDefinition() {
                                Id = "https://www.spid.gov.it/openid-federation/agreement/sp-public/",
                                Issuer = "http://127.0.0.1:8000/",
                                TrustMark = "eyJhbGciOiJSUzI1NiIsImtpZCI6ImRCNjdnTDdjazNURmlJQWY3TjZfN1NIdnFrME1EWU1FUWNvR0dsa1VBQXciLCJ0eXAiOiJ0cnVzdC1tYXJrK2p3dCJ9.eyJpc3MiOiJodHRwOi8vMTI3LjAuMC4xOjgwMDAvb2lkYy9vcC8iLCJzdWIiOiJodHRwOi8vMTI3LjAuMC4xOjUwMDAvIiwiaWF0IjoxNjQ4NTYwMjgzLCJpZCI6Imh0dHBzOi8vd3d3LnNwaWQuZ292Lml0L2NlcnRpZmljYXRpb24vcnAiLCJtYXJrIjoiaHR0cHM6Ly93d3cuYWdpZC5nb3YuaXQvdGhlbWVzL2N1c3RvbS9hZ2lkL2xvZ28uc3ZnIiwicmVmIjoiaHR0cHM6Ly9kb2NzLml0YWxpYS5pdC9pdGFsaWEvc3BpZC9zcGlkLXJlZ29sZS10ZWNuaWNoZS1vaWRjL2l0L3N0YWJpbGUvaW5kZXguaHRtbCJ9.M6T42JXb9wHBhwy2cueHEFoMNcaHQZKvMTMR3aavZVBvW14hps_IZ_MT3yqA5wTEZTgAC_-M8G33wjpTMw26ITXgOW6rMUqWHWj7639BfbqnGkoZdMuMxo96nSOIaxXElvfPRZu6wQ9LOrXe_kyR3eo6p8iZLKbnp1e1D5VTr_dSEYQsaTlVmiT6I2SyaiWtpXCD1DWZxNw2YKTie0lEmDCMO4WJo3kfr_ak9kvMryF8-5crecOs6o33DYGR5zSzJ3JQYAz2huLKZ_y7nzmvkEjDQNjtg1R2cusNHvxLt8Su3T0hUbT_Vl5-b_VvBqo2yUTc7Z7WOJPwzId8rATujA"
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
