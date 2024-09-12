# SPID/CIE OIDC Federation SDK for AspNetCore

![aspnetcore-versions](https://img.shields.io/badge/aspnetcore-8.0%20%7C%207.0%20%7C%206.0%20%7C%205.0%20%7C%203.1-brightgreen)
![CI build](https://github.com/italia/spid-cie-oidc-aspnetcore/workflows/spid_cie_oidc/badge.svg)
![badge](https://img.shields.io/endpoint?url=https://raw.githubusercontent.com/italia/spid-cie-oidc-aspnetcore/main/.github/coverage_badge.json)
![Apache license](https://img.shields.io/badge/license-Apache%202-blue.svg)
[![GitHub issues](https://img.shields.io/github/issues/italia/spid-cie-oidc-aspnetcore.svg)](https://github.com/italia/spid-cie-oidc-aspnetcore/issues)

[![Get invited](https://slack.developers.italia.it/badge.svg)](https://slack.developers.italia.it/)
[![Join the #spid openid](https://img.shields.io/badge/Slack%20channel-%23spid%20openid-blue.svg)](https://developersitalia.slack.com/archives/C7E85ED1N/)

The SPID/CIE OIDC Federation Relying Party SDK, written in C# for AspNetCore. This is a custom implementation of an AspNetCore RemoteAuthenticationHandler for the OpenIdConnect profiles of SPID and CIE.

## Summary

* [Features](#features)
* [Setup](#setup)
* [Extensions](#extensions)
* [Error Handling](#error-handling)
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
        // ......

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
            
            // ......
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        // ....

        // This method MUST be called before UseAuthentication()
        app.UseSpidCieOIDC();

        app.UseAuthentication();
        app.UseAuthorization();
       
        // ....
    }
```

In this way, the necessary middlewares for the management of login/logout requests/responses from/to SPID/CIE are added, together with the middeware responsible of the OIDC-Federation endpoint management. These middlewares add to the webapp the `/signin-spidcie` and `/signout-spidcie` endpoints on which the library listens to interpret the Login and Logout responses respectively coming from the SPID/CIE IdentityProviders. These endpoints, in their absolute URL, and therefore inclusive of schema and hostname will be something like `https://webapp.customdomain.it/signin-spidcie` and `https://webapp.customdomain.it/signout-spidcie` (or `https://webapp.customdomain.it/<id_code>/signin-spidcie` and `https://webapp.customdomain.it/<id_code>/signout-spidcie` in case of an aggregated subject).
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

It's possible to configure the library by reading the configuration settings from the appsettings.json file, using the command

```csharp
o.LoadFromConfiguration(Configuration);
```

In particular, a 'SpidCie' section can be added to the configuration which has the following format
```json
  "SpidCie": {
    "RequestRefreshToken": true,
    "SpidOPs": [ "http://trust-anchor.org:8000/oidc/op" ],
    "CieOPs": [ "http://cie-provider.org:8002/oidc/op" ],
    "RelyingParties": [
      {
        "Id": "http://aspnetcore.relying-party.org:5000/",
        "Name": "RP Test",
        "OrganizationName": "RP Test",
        "OrganizationType": "Public", // or Private
        "HomepageUri": "http://aspnetcore.relying-party.org:5000/",
        "PolicyUri": "http://aspnetcore.relying-party.org:5000/",
        "LogoUri": "http://aspnetcore.relying-party.org:5000/",
        "SecurityLevel": 2,
        "Contacts": [ "info@rptest.it" ],
        "AuthorityHints": [ "http://trust-anchor.org:8000" ],
        "LongSessionsEnabled": true,
        "RequestedClaims": [
          "Name",
          "FamilyName",
          "Email",
          "FiscalNumber",
          "DateOfBirth",
          "PlaceOfBirth"
        ],
        "TrustMarks": [
          {
            "Id": "https://www.spid.gov.it/openid-federation/agreement/sp-public/",
            "TrustMark": "eyJhbGc...."
          }
        ],
        "OpenIdCoreCertificates": [
          {
            "Source": "File", // Or "Raw"
            "File": {
              "Path": "wwwroot/certificates/ComuneVigata-SPID.pfx", // This certificate is for demo only. Don't use this certificate for production or onboarding purposes
              "Password": "P@ssW0rd!"
            },
            "Raw": {
              "Certificate": "base64",
              "Password": "password"
            }
          }
        ],
        "OpenIdFederationCertificates": [
          {
            "Source": "File", // Or "Raw"
            "File": {
              "Path": "wwwroot/certificates/ComuneVigata-SPID.pfx", // This certificate is for demo only. Don't use this certificate for production or onboarding purposes
              "Password": "P@ssW0rd!"
            },
            "Raw": {
              "Certificate": "base64",
              "Password": "password"
            }
          }
        ]
      }
    ],
    "Aggregators": [
      {
        "Id": "http://aspnetcore.aggregator.org:5000/",
        "Name": "Aggregator Test",
        "OrganizationName": "Aggregator Test",
        "HomepageUri": "http://aspnetcore.aggregator.org:5000/",
        "PolicyUri": "http://aspnetcore.aggregator.org:5000/",
        "LogoUri": "http://aspnetcore.aggregator.org:5000/",
        "Contacts": [ "info@aggrtest.it" ],
        "OrganizationType": "Private", // or Public
        "Extension": "Full", // or Light
        "AuthorityHints": [ "http://trust-anchor.org:8000" ],
        "TrustMarks": [
          {
            "id": "https://preprod.oidc.registry.servizicie.interno.gov.it/intermediate/private",
            "issuer": "https://preprod.oidc.registry.servizicie.interno.gov.it"
            "trust_mark": "eyJhbGc...."
          }
        ],
        "OpenIdFederationCertificates": [
          {
            "Source": "File", // Or "Raw"
            "File": {
              "Path": "wwwroot/certificates/ComuneVigata-SPID.pfx", // This certificate is for demo only. Don't use this certificate for production or onboarding purposes
              "Password": "P@ssW0rd!"
            },
            "Raw": {
              "Certificate": "base64",
              "Password": "password"
            }
          }
        ],
        "MetadataPolicy": {},
        "RelyingParties": [
          {
            "AuthorityHints": [
                "http://aspnetcore.aggregator.org:5000/"
            ],
            "Id": "http://aspnetcore.aggregator.org:5000/TestRP/",
            "Name": "RP Test",
            "OpenIdCoreCertificates": [
                {
                    "Algorithm": "RS256", //Or RSA-OAEP-256
                    "Certificate": "base64",
                    "KeyUsage": "Signature" //Or Encryption
                }
            ],
            "OrganizationName": "RP Test",
            "OrganizationType": "Public", // or Private
            "HomepageUri": "http://aspnetcore.aggregator.org:5000/TestRP/",
            "PolicyUri": "http://aspnetcore.aggregator.org:5000/TestRP/",
            "LogoUri": "http://aspnetcore.aggregator.org:5000/TestRP/",
            "SecurityLevel": 2,
            "Contacts": [ "info@rptest.it" ],
            "LongSessionsEnabled": true,
            "RedirectUris": [
                "http://aspnetcore.aggregator.org:5000/TestRP/signin-oidc-spidcie"
            ]
            "RequestedClaims": [
              "Name",
              "FamilyName",
              "Email",
              "FiscalNumber",
              "DateOfBirth",
              "PlaceOfBirth"
            ],
            "SecurityLevel": "L1", //Or L2 or L3
            "TrustMarks": [
              {
                "Id": "https://registry.interno.gov.it/openid_relying_party/public/",
                "Issuer": "http://aspnetcore.aggregator.org:5000",
                "TrustMark": "eyJhbGc...."
              }
            ]
          }
        ]
      }
    ]
  }
```
The configuration of the RP/SA certificates is done by specifying in the `Source` field one of the values `File/Raw` and by filling in the section corresponding to the specified value. The sections not used (e.g. those corresponding to the other values) can be safely deleted from the configuration file, since they will not be read.
Alternatively, you can configure all of the above options programmatically, from the `AddSpidCieOIDC(options => ...)` method.

## Extensions
The library comes with a set of default services that you could override in order to have a different desired behavior for the related core features.
In particular, you could implement the following interfaces:
- `ILogPersister`: you should implement this interface in order to intercept the OIDC Requests and Responses to and from the OPs. The implementation should encapsulate the persistence strategy of the request and response logs, as per the Spid/Cie Technical Rules. (mandatory)
- `IIdentityProvidersRetriever`: you can override the default implementation for this interface if you aim to retrieve at run-time the list of the OPs (e.g. from an external configuration file, from a Database, etc.) whose trust chains have to be built during the initialization phase and rendered in the Spid/Cie login buttons. (optional)
- `IRelyingPartiesRetriever`:  you can override the default implementation for this interface if you aim to retrieve at run-time the list of the RelyingParties managed by the library (e.g. from an external configuration file, from a Database, etc.). This is useful in a multi-tenant scenario, where you want to handle multiple RPs with a single webapp. (optional)

The library allows to register the custom implementations by using the following extension methods:

```csharp
services.AddSpidCieOIDC(o => /* ..... */ )
   .AddLogPersister<CustomLogPersister>()
   .AddIdentityProvidersRetriever<CustomIdentityProvidersRetriever>()
   .AddRelyingPartiesRetriever<CustomRelyingPartiesRetriever>()
```

## Error Handling
The library can, at any stage (both in the Request creation stage and in the Response management stage), raise exceptions.
A typical scenario is the one in which the error codes provided by the SPID protocol are received (n.19, n.20, etc ....), in this case the library raises an exception containing the corresponding localized error message, required by the SPID specifications, which can be managed (for example for visualization) using the normal flow provided for AspNetCore. The following example uses AspNetCore's ExceptionHandling middleware.

```csharp
public void Configure(IApplicationBuilder app, IHostEnvironment env)
{
    ...
    app.UseExceptionHandler("/Home/Error");
    ...
}

.......

// HomeController
[AllowAnonymous]
public async Task<IActionResult> Error()
{
    var exceptionHandlerPathFeature =
        HttpContext.Features.Get<IExceptionHandlerPathFeature>();

    string errorMessage = string.Empty;

    if (exceptionHandlerPathFeature?.Error != null)
    {
        var messages = FromHierarchy(exceptionHandlerPathFeature?.Error, ex => ex.InnerException)
            .Select(ex => ex.Message)
            .ToList();
        errorMessage = String.Join(" ", messages);
    }

    return View(new ErrorViewModel
    {
        RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
        Message = errorMessage
    });
}

private IEnumerable<TSource> FromHierarchy<TSource>(TSource source,
            Func<TSource, TSource> nextItem,
            Func<TSource, bool> canContinue)
{
    for (var current = source; canContinue(current); current = nextItem(current))
    {
        yield return current;
    }
}

private IEnumerable<TSource> FromHierarchy<TSource>(TSource source,
    Func<TSource, TSource> nextItem)
    where TSource : class
{
    return FromHierarchy(source, nextItem, s => s != null);
}
```

## Sample WebApp
For the Sample WebApp doc and how to do a sample Onboarding with the Django OP and TA, please [read this section](SAMPLE.md)

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
  - Both RP and SA are supported
- Multitenancy, a single service can configure many RPs
- Bootstrap Italia Design templates


## License and Authors

This software is released under the Apache 2 License by:

- Daniele Giallonardo
- Giulio Maffei (A Software Factory s.r.l.)



## Acknowledgments
A special thank to "A Software Factory s.r.l." for their valuable support and contributions to this project. Their technical assistance and resources have been instrumental in advancing and optimizing our work.

We appreciate their expertise and commitment, which have significantly enhanced the quality and innovation of the project.
