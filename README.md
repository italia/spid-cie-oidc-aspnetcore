# SPID/CIE OIDC Federation SDK for AspNetCore

![CI build](https://github.com/italia/spid-cie-oidc-aspnetcore/workflows/spid_cie_oidc/badge.svg)
![badge](https://img.shields.io/endpoint?url=https://raw.githubusercontent.com/italia/spid-cie-oidc-aspnetcore/main/.github/coverage_badge.json)
![Apache license](https://img.shields.io/badge/license-Apache%202-blue.svg)
![aspnetcore-versions](https://img.shields.io/badge/aspnetcore-3.1%20%7C%205.0%20%7C%206.0-brightgreen)
[![GitHub issues](https://img.shields.io/github/issues/italia/spid-cie-oidc-aspnetcore.svg)](https://github.com/italia/spid-cie-oidc-aspnetcore/issues)
[![Get invited](https://slack.developers.italia.it/badge.svg)](https://slack.developers.italia.it/)
[![Join the #spid openid](https://img.shields.io/badge/Slack%20channel-%23spid%20openid-blue.svg)](https://developersitalia.slack.com/archives/C7E85ED1N/)

> ⚠️ __This project is a WiP, the first stable release for production use will be the v1.0.0.__

The SPID/CIE OIDC Federation Relying Party SDK, written in C# for AspNetCore. This is a custom implementation of an AspNetCore RemoteAuthenticationHandler for the OpenIdConnect profiles of SPID and CIE.

## Summary

* [Features](#features)
* [Setup](#setup)
* [Extensions](#extensions)
* [Sample WebApp](#sample-webapp)
* [Contribute](#contribute)
    * [Contribute as end user](#contribute-as-end-user)
    * [Contribute as developer](#contribute-as-developer)
* [Implementations notes](#implementation-notes)
* [License and Authors](#license-and-authors)

## Features
The purpose of this project is to provide a simple and immediate tool to integrate, in a WebApp developed with AspNetCore MVC, the authentication services of SPID and CIE, automating the login/logout flows, the management of the OIDC-Core/OIDC-Federation protocols and their security profiles, and simplify the development activities.
Inside the repository there is both the library code (Spid.Cie.OIDC.AspNetCore), and a demo web app (Spid.Cie.AspNetCore.WebApp) which shows a sample integration of the library into an AspNetCore MVC webapp.

## Setup

The library is distributed in the form of a NuGet package, which can be installed via the command

`Install-Package Spid.Cie.OIDC.AspNetCore`

At this point it is sufficient, inside the `Startup.cs`, to add the following lines:

```csharp
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
                o.SpidOPs.Add("http://127.0.0.1:8000/oidc/op/"); // Example SPID OP Url
                o.CieOPs.Add("http://127.0.0.1:8002/oidc/op/"); // Example CIE OP Url
                o.RelyingParties.Add(new RelyingParty()
                {
                    // .... RelyingParty configuration
                });
            });
            
            // ......
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        // ....
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
        
        // ....
    }
```

In this way, the necessary middlewares for the management of login/logout requests/responses from/to SPID/CIE are added, together with the middeware responsible of the OIDC-Federation endpoint management. These middlewares add to the webapp the `/signin-spidcie` and `/signout-spidcie` endpoints on which the library listens to interpret the Login and Logout responses respectively coming from the SPID/CIE IdentityProviders. These endpoints, in their absolute URL, and therefore inclusive of schema and hostname will be something like `https://webapp.customdomain.it/signin-spidcie` and `https://webapp.customdomain.it/signout-spidcie`.
Note that, according to the SPID/CIE guidelines, a compliant self-signed X509 certificate could be used both for the OIDC-Federation and OIDC-Core purposes (better if you use different certificates for Core and Fed). You could generate one for your RP using the Spid Compliant Certificate tool (https://github.com/italia/spid-compliant-certificates) or any tool you prefer that let you generate a compliant certificate.

The library also includes the implementation of a set of TagHelpers for the rendering (conforming to the specifications) of the "Enter with SPID" and "Enter with CIE" buttons. The rendering of the buttons will be performed after the trust chains with the OPs (previously configured in the Startup) will be established successfully upon the first request. To render the buttons simply add the following code to the Razor View where you want to place them:

```razor
@using Spid.Cie.OIDC.AspNetCore.WebApp
@using Spid.Cie.OIDC.AspNetCore.WebApp.Models
@using Spid.Cie.OIDC.AspNetCore.Mvc
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
@addTagHelper *, Spid.Cie.OIDC.AspNetCore

@section styles {
	<style spid></style>
}

<spid-button challenge-url="/home/login" size="Medium" class="text-left"></spid-button>
<cie-button challenge-url="/home/login" size="Medium" class="text-left"></cie-button>

@section scripts {
	<script spid></script>
}
```

The `spid-button` and the `cie-button` TagHelpers will automatically generate the HTML code needed to render the OPs list that have been successfully trusted in the initialization phase. The `size` attribute can be set with the values` Small, Medium, Large, ExtraLarge`.
`<style spid> </style>` and `<script spid> </script>` instead represent the TagHelpers for rendering, respectively, the CSS classes and the JS code needed to execute the button.

## Extensions
The library comes with a set of default services that you could override in order to have a different desired behavior for the related core features.
In particular, you could implement the following interfaces:
- `ILogPersister`: you should implement this interface in order to intercept the OIDC Requests and Responses to and from the OPs. The implementation should encapsulate the persistence strategy of the request and response logs, as per the Spid/Cie Technical Rules. (mandatory)
- `IIdentityProvidersRetriever`: you can override the default implementation for this interface if you aim to retrieve at run-time the list of the OPs whose trust chains have to be built during the initialization phase and rendered in the Spid/Cie login buttons. (optional)
- `IRelyingPartiesRetriever`:  you can override the default implementation for this interface if you aim to retrieve at run-time the list of the RelyingParties managed by the library. This is useful in a multi-tenant scenario, where you want to handle multiple RPs with a single webapp. (optional)
- `IRelyingPartySelector`: you can override the default implementation for this interface if you want to customize the "currently selected RP" strategy during the Challenge phase. The default implementation takes the first configured RP, and it should be enough in a single-tenant scenario. You definitely want to specify a proper strategy in a multi-tenant scenario (e.g. using a custom query-string parameter strategy, a cookie strategy, etc.) (optional)

The library allows to register the custom implementations by using the following extension methods:

```csharp
services.AddSpidCieOIDC(o => /* ..... */ )
   .AddLogPersister<CustomLogPersister>()
   .AddIdentityProvidersRetriever<CustomIdentityProvidersRetriever>()
   .AddRelyingPartiesRetriever<CustomRelyingPartiesRetriever>()
   .AddRelyingPartySelector<CustomRelyingPartySelector>()
```


## Sample WebApp

> TODO: WiP

![image](https://user-images.githubusercontent.com/58780951/160617044-125cd807-1e89-4eb6-8dc7-a5e2e40b268b.png)

![image](https://user-images.githubusercontent.com/58780951/160616885-4047c644-d017-46b3-b68f-4d09dd986877.png)

http://127.0.0.1:8000/admin

![image](https://user-images.githubusercontent.com/58780951/160620799-bd977e76-5a8e-4b70-a18c-5648243afeea.png)

![image](https://user-images.githubusercontent.com/58780951/160620963-ee03114b-ca04-4fa0-9f26-9573d9f31bf5.png)

![image](https://user-images.githubusercontent.com/58780951/160620988-514d9e4e-3dec-4a16-b10a-a22ccbd8f8d2.png)

![image](https://user-images.githubusercontent.com/58780951/160621023-d222ba8b-3a6c-4c00-9062-4651175db02d.png)

![image](https://user-images.githubusercontent.com/58780951/160621121-ea73f138-2a04-4a9c-b4b9-6999c8e47498.png)

![image](https://user-images.githubusercontent.com/58780951/160621147-2827852b-671d-4393-9169-f5705aad701f.png)

![image](https://user-images.githubusercontent.com/58780951/160621175-d9c08f37-90bd-4205-a329-c49c15258fe4.png)

![image](https://user-images.githubusercontent.com/58780951/160621198-d2fc1556-9172-4e6a-aa33-f9d708d6ae92.png)

![image](https://user-images.githubusercontent.com/58780951/160621223-e1effd4a-6ff3-469a-a766-12d006a6ae54.png)

![image](https://user-images.githubusercontent.com/58780951/160621237-e0b1f393-5ebc-4278-bb2c-60a402f5767b.png)

![image](https://user-images.githubusercontent.com/58780951/160621262-a6cabccd-1d98-45fd-b5fa-5235b5995b3b.png)

![image](https://user-images.githubusercontent.com/58780951/160621298-5b4dbc90-e470-4017-9462-831e2251c375.png)

![image](https://user-images.githubusercontent.com/58780951/160621324-0af80048-4f72-4bcd-8538-e891c66cfe34.png)

![image](https://user-images.githubusercontent.com/58780951/160621347-a427f49f-2a8e-4c13-a94f-632e177a1c30.png)

![image](https://user-images.githubusercontent.com/58780951/160621370-f00f8b50-f3ef-4edc-a460-92cc641cd102.png)

![image](https://user-images.githubusercontent.com/58780951/160621452-98380f1d-edcf-4ec9-9148-5656a76ed217.png)

![image](https://user-images.githubusercontent.com/58780951/160621644-ba55d7e6-702e-4e59-b0a0-695020cf3fb0.png)

![image](https://user-images.githubusercontent.com/58780951/160621672-966aed34-e777-443a-a0eb-f92ed7cf857c.png)

![image](https://user-images.githubusercontent.com/58780951/160621694-72a75c4f-3023-4d9e-98ea-0d22abcd5dbd.png)

![image](https://user-images.githubusercontent.com/58780951/160621716-0f425bc4-7260-401d-9490-174797769eaf.png)

![image](https://user-images.githubusercontent.com/58780951/160621737-00090f62-d916-4f8b-a213-512fd87f102d.png)



## Contribute

Your contribution is welcome, no question is useless and no answer is obvious, we need you.

#### Contribute as end user

Please open an issue if you've discovered a bug or if you want to ask some features.

#### Contribute as developer

Please open your Pull Requests on the __dev__ branch. 
Please consider the following branches:

 - __main__: where we merge the code before tag a new stable release.
 - __dev__: where we push our code during development.
 - __other-custom-name__: where a new feature/contribution/bugfix will be handled, revisioned and then merged to dev branch.

In this project we adopt [Semver](https://semver.org/lang/it/) and
[Conventional commits](https://www.conventionalcommits.org/en/v1.0.0/) specifications.

## Implementation notes

This project proposes an implementation of the italian OIDC Federation profile with
__automatic_client_registration__ and the adoption of the trust marks as mandatory.

#### General Features

- SPID and CIE OpenID Connect Relying Party
- OIDC Federation 1.0
  - Automatic OPs discovery 
  - Trust chain storage and discovery
  - Build trust chains for all the available OPs
- Multitenancy, a single service can configure many RPs
- Bootstrap Italia Design templates


## License and Authors

This software is released under the Apache 2 License by:

- Daniele Giallonardo <danielegiallonardo83@gmail.com>.
