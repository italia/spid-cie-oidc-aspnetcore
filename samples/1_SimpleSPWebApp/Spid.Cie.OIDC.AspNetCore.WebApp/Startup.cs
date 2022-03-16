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
                o.DefaultChallengeScheme = SpidCieDefaults.AuthenticationScheme;
            })
            .AddCookie()
            .AddSpidCieOIDC(o =>
            {
                o.LoadFromConfiguration(Configuration);

                o.RelyingParties = new System.Collections.Generic.List<RelyingParty>() {
                    new RelyingParty()
                    {
                        ClientId = "http://127.0.0.1:5000/",
                        ClientName = "RP Test",
                        Contacts = new string[] { "info@rptest.it" },
                        Issuer = "http://127.0.0.1:5000/",
                        AuthorityHints = new string[] { "http://127.0.0.1:8000/" },
                        RedirectUris = new string[] { "http://127.0.0.1:5000/signin-oidc" },
                        SecurityLevel = SecurityLevel.L2,
                        LongSessionsEnabled = false,
                        TrustMarks = new TrustMarkDefinition[] {
                            new TrustMarkDefinition() {
                                Id = "https://www.spid.gov.it/openid-federation/agreement/sp-public/",
                                Issuer = "http://127.0.0.1:8000/",
                                TrustMark = "eyJhbGciOiJSUzI1NiIsImtpZCI6IkZpZll4MDNibm9zRDhtNmdZUUlmTkhOUDljTV9TYW05VGM1bkxsb0lJcmMiLCJ0eXAiOiJ0cnVzdC1tYXJrK2p3dCJ9.eyJpc3MiOiJodHRwOi8vMTI3LjAuMC4xOjgwMDAvIiwic3ViIjoiaHR0cDovL2xvY2FsaG9zdDo1MDAwLyIsImlhdCI6MTY0NzI3Njc2NiwiaWQiOiJodHRwczovL3d3dy5zcGlkLmdvdi5pdC9jZXJ0aWZpY2F0aW9uL3JwIiwibWFyayI6Imh0dHBzOi8vd3d3LmFnaWQuZ292Lml0L3RoZW1lcy9jdXN0b20vYWdpZC9sb2dvLnN2ZyIsInJlZiI6Imh0dHBzOi8vZG9jcy5pdGFsaWEuaXQvaXRhbGlhL3NwaWQvc3BpZC1yZWdvbGUtdGVjbmljaGUtb2lkYy9pdC9zdGFiaWxlL2luZGV4Lmh0bWwifQ.uTbO9gbx3cyNgs4LS-zij9kOC1alQuxFytsPNjwloGdnoGj_4PCJasMxmKVyUJXkXKQGeiG69oXBnf6sL9McYP6RYklhqFBR0hW4X5H5qc4vDYetDo8ajzocMZm050YzTrUObwy3OLOQRGLuWvg2uifRy8YCC0xD0OxoeBaEeURM_zkU3PFQ76RLP2W8b63J37behBevrO1lKJHhyfE4oJ6qFpR2Vk0367mMu7c0vhuTZYw8a5UkDbYR4L77vyzVlpE1duL5ibvREV4YMuMtWbI9fn1nlpgtmTp1Z089PN_PHVQHBrmHRG6jcwU6JCOdNXFBTsXtglU-xRng99Z6aQ"
                            }
                        },
                        OpenIdCoreJWKs = new Microsoft.IdentityModel.Tokens.JsonWebKeySet("{ 'keys': [{\"kty\":\"RSA\",\"kid\":\"BCCECDCBBEA467398633B1EA009C9485\",\"e\":\"AQAB\",\"n\":\"5UydWJp9a4uItfOF6WkBA-SSmq93_7eriWNKb_NDqmmNg3VEUG6YZCs47C-QYxLmJ8O0jd8V-0FIjiYrLPKhowBuZqe88MkrcWZGLBUb7Nabvh7NppHqcan42Ely3Xlltq8YO8uIlV3qyNZYNguezdBKhyBGtMmf5b2-df6jSOKthSeY54d66C-LgHtxkAfFJBwbk4cl79tHUSP30ItK6jGZdWHYnZr1PpC2I0sI2trVvtFvfc-kLCkybAgvkbqM04FzcxpGynTNGpKzRU069O8gPTuK2mL4NFXgOzQFKNVuk0v3Dgc1mN8c4ijszsevgWE3fQF60m_7nRVMvdGJcQ\",\"d\":\"nkuM2E0Wxna2cz3htWf6_m_-UXFPOybV8eusyKo8jVl_C0CaX5hp9cTs8AhJ-ktivhLaA5L9fs3rw85PwiDiO0Ah9xZXVjbamdeMHASamZ7yN4bWW-ah3cQEeXQDKygSctJfvW_eI5eJbQqkLPNKtzTTLoO2rDoA-75IzPZ0TOx5boxb1G6wcPjyGe7LKXa70QR-B7iTNyozqbs21pyNp--cQ5YUos19ZLLcSQ6fC1wNzmQuYLnrRH9ekVJzdJn7MxWMkHv_xhHtqNSQ8hs4czS6lr9aWa9pt-EXWRO7N5rTP_MfRrVnyShHDE2dUzv0GgUik2VGY79M-yqi7c7RvQ\",\"p\":\"8mz7E_Da6F99zRFZ3Og5utBLaVmb6-lAwYAvCOmdB1RerEKUkHQs4-a2RE0GSLF20RPTzKz0TN31WGM5Bw8qsS0F-xLm_C_uVK9qgX0a7WUx43Go-4akjXPvDQyL6YPZ2c6WQgmLPd_oyEByq-2bfv1hzi23FLK7Ial99MemVXM\",\"q\":\"8iN5i01VjytOljoHPwse2eBiKoEWAQfnFLk3EIlEHz6iFp3INIFkyXbcCli17ZTL8DGNOpWOR7yu_l-883OWZ-RKKl04i-rY1anjwhgRj4p4ZMkd-wzebfgxpBZE0FpPOTNrLJt7_j_BNA2p3SEx8TAMoDiKD-KTWCE1-XWRTIs\",\"dp\":\"Nqk9_6QKJ-UIH4nvAjFWevnmVw2-a3X_hOHbOR47quBqLFsi1mNrj8OAi2v2o8Nn8AKReg_xUbemT1SoEiBoVuS4-YCslmZUTcHzuNi1jpuHoSoKmRQl3EMsvnt6vJ9fKo47MQ6n17655RUrBfsgWYTWXb_PDRqzuQZXgS7XIWM\",\"dq\":\"m84L1YDrfvZE2RF5vC0xqsxhHix1tjAZlxIexnss5FXvAlKAkph3-9KJf_bZCYjnOSUJVRsKtEpK588-zAAiVbNlraDSU-XHpKRus2O9WPmmwmNO8U20ilpbxtO1b8PMmmflnIxn2o-3iAEKvgkwE5vxY989pz8JxHmKO3xVBEU\",\"qi\":\"FLZYS55hkXLMre4aUp_IduJe3_i6WesaOzgNOx96pzGin01w2WA63f_teh8m4sGbjTrJprIH-zxbAbJEV4wf_TFD-kh-k63jwPn7KBJh19zv1EL_7snmsExn6K2xpsXQrgCv89yy5QDkkVImpyWXNGTipQKxDMb-8ile8OuGtoE\"}]}"),
                        OpenIdFederationJWKs = new Microsoft.IdentityModel.Tokens.JsonWebKeySet("{ 'keys': [{\"kty\":\"RSA\",\"kid\":\"11B008CC96393E8E7E6577358747D177\",\"e\":\"AQAB\",\"n\":\"4FFtq5_50wLUoe2t-UYA_Z_Zp9HLZqE2hW45mGA2q8EIam_sfNyFRMarktLNeboyUVgl3bqsz9RuHR3UqifgrVK-Jgns0hpsjr4FAlBhfTEDz_keeGhbGfRuNl3S-6bVxh2olqOeUNXVbw1lRs9oMjnz_xbS73G_dli6INgD_jnUcitHrvhTAn6WRrF2Ism9j_HDLm2f1e3dhBmys5VGFAw_iVL4NelG2vbJBepUfYkMA7lpAGGKU86lxauHww8Hs36ncuewgYsSG6NOoyTxevznJiUdjJAIWmvld_ElaJxj39bfhqOks8OEFoTQoXqwVHb1FDdOBP9Q97ep8NQtzQ\",\"d\":\"kWOVrpBQjvidsppnkVJTIA-0NfZfXBSPTupDQETUCnxhpQMIB2GyqwINgGtLLGOHa3uzh0QP7-BiwlJsXSDdmdkwT11NdlwUEmyMuJQ6q7bSHsD7g9U3ftoTeP9U2TCoC_F7LK-90-zUmlbXVo7lnPA8-g_kBbf2_ZDCirkX1XSVXm_CjllV2pRjG7mWNK-godw0ZedT7nCaeKmg3fgPvZ2n7Xw91dgGP5BqFRH89Iox-FgKDMyYfem4J4Re8Oo1M62J7OZEGni7YUupgo33IFEtYWud_1kK3UDxgUhQUKKQk3hF1v6XhOmK1IPo3EUAx_HTl6Ye_JN59KxYvgpkTQ\",\"p\":\"7qrrMsUwXzIf19jIeMcE_iYpb4TwRjOG2cju7E1hpUplLEdRtih5p4vfJmDv6EtOBJQD8jPVhkW_NeV6DQgsmOZ92A437pqDneFjR01-rVGjdCuCTlHUqbFC7Q9H0taOrUAriR_GqCD6gNVMXv0EKRho7Xwz18wX_sExe1DCRZc\",\"q\":\"8Ju8cc871AqNIL_uOKqeOPh8YbQuzdBxoNeI2yVXfslXxwe2eDoPKbd47AL9XRJiVSw1bz-YMC4V3HWCh-1Nwdg6A0Z6p8WZeNTzmRpu8sGGKF6rLZ1-IVk0Kd6NdQORo3hpY_ds3fwAJC3Ef40anpCKbOhGJoOGR9PyuN2PfDs\",\"dp\":\"TrR2k2uLpBj3f0qx8YJQFy-mgmwogD4FvlQQ1kQ-ay-5ZXyvaY63b5UkpZdaBhSvSbs2Ae0uAPWHNNLUCPAlJNb9sP4fW8QAm1P0VVMs7yL48BpZAlLh-oPGOj7LxK-UpXV_5dxgupkFgu7UHI77jEHMeGR5BDT9xLkZBD2Bk0k\",\"dq\":\"scbwEstC8mYlZohJlNcD9yYqiOpgFrQn8Oav-PP7VPOhhq59NRH4-CLXFtMSrz0RKMt0Y9GCOj8i6fRtUM-Wv15rZtYhdGr8_Zmir4InbfhtjtB7_EU815kYgMMuk8HiTv1-KV28s7wpwpGKeZAhlf1IOjXY90YA5nuFAPC7vtc\",\"qi\":\"L9NPt5G2nnezZPDXqIJy-usBVVk_0gkBPwBND74mS3ZDtmB4IKaJb7OqW8LQpeyYACCxTea7KMhefi4AmPR4T02bCOE1NwGUyT2So_LDe3YygVZRJ_roTwE-BueNKHpa1TH45k28NgO_FYx04hHZSP63hC7-6dVPqob73u7ZuAg\"}]}"),
                        RequestedClaims = new ClaimTypes[] {
                            ClaimTypes.Name,
                            ClaimTypes.FamilyName,
                            ClaimTypes.Email,
                            ClaimTypes.FiscalNumber,
                            ClaimTypes.DateOfBirth,
                            ClaimTypes.PlaceOfBirth
                        }
                    }
                };
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
