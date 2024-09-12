using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Spid.Cie.OIDC.AspNetCore.Configuration;
using Spid.Cie.OIDC.AspNetCore.Enums;
using Spid.Cie.OIDC.AspNetCore.Helpers;
using Spid.Cie.OIDC.AspNetCore.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Xunit;

namespace Spid.Cie.OIDC.AspNetCore.Tests.IntegrationTests;

internal partial class TestSettings
{
    private readonly Action<SpidCieOptions> _configureOptions;
    private SpidCieOptions _options;

    public TestSettings() : this(configure: null)
    {
    }

    public TestSettings(Action<SpidCieOptions> configure)
    {
        var certificate = new X509Certificate2("ComuneVigata-SPID.pfx", "P@ssW0rd!");
        _configureOptions = o =>
        {
            _options = o;
            _options.RequestRefreshToken = true;
            _options.SpidOPs.Add("http://127.0.0.1:8000/oidc/op/");
            _options.CieOPs.Add("http://127.0.0.1:8002/oidc/op/");
            _options.RelyingParties.Add(new RelyingParty()
            {
                Id = "http://127.0.0.1:5000/",
                Name = "RP Test",
                HomepageUri = "http://127.0.0.1:5000/",
                LogoUri = "http://127.0.0.1:5000/",
                OrganizationName = "RP Test",
                PolicyUri = "http://127.0.0.1:5000/",
                Contacts = new() { "info@rptest.it" },
                AuthorityHints = new() { "http://127.0.0.1:8000/oidc/op/" },
                RedirectUris = new() { $"http://127.0.0.1:5000{SpidCieConst.CallbackPath}" },
                SecurityLevel = SecurityLevels.L2,
                LongSessionsEnabled = false,
                TrustMarks = new()
                {
                    new TrustMarkDefinition()
                    {
                        Id = "https://www.spid.gov.it/openid-federation/agreement/sp-public/",
                        TrustMark = "eyJhbGciOiJSUzI1NiIsImtpZCI6IkZpZll4MDNibm9zRDhtNmdZUUlmTkhOUDljTV9TYW05VGM1bkxsb0lJcmMiLCJ0eXAiOiJ0cnVzdC1tYXJrK2p3dCJ9.eyJpc3MiOiJodHRwOi8vMTI3LjAuMC4xOjgwMDAvIiwic3ViIjoiaHR0cDovL2xvY2FsaG9zdDo1MDAwLyIsImlhdCI6MTY0NzI3Njc2NiwiaWQiOiJodHRwczovL3d3dy5zcGlkLmdvdi5pdC9jZXJ0aWZpY2F0aW9uL3JwIiwibWFyayI6Imh0dHBzOi8vd3d3LmFnaWQuZ292Lml0L3RoZW1lcy9jdXN0b20vYWdpZC9sb2dvLnN2ZyIsInJlZiI6Imh0dHBzOi8vZG9jcy5pdGFsaWEuaXQvaXRhbGlhL3NwaWQvc3BpZC1yZWdvbGUtdGVjbmljaGUtb2lkYy9pdC9zdGFiaWxlL2luZGV4Lmh0bWwifQ.uTbO9gbx3cyNgs4LS-zij9kOC1alQuxFytsPNjwloGdnoGj_4PCJasMxmKVyUJXkXKQGeiG69oXBnf6sL9McYP6RYklhqFBR0hW4X5H5qc4vDYetDo8ajzocMZm050YzTrUObwy3OLOQRGLuWvg2uifRy8YCC0xD0OxoeBaEeURM_zkU3PFQ76RLP2W8b63J37behBevrO1lKJHhyfE4oJ6qFpR2Vk0367mMu7c0vhuTZYw8a5UkDbYR4L77vyzVlpE1duL5ibvREV4YMuMtWbI9fn1nlpgtmTp1Z089PN_PHVQHBrmHRG6jcwU6JCOdNXFBTsXtglU-xRng99Z6aQ"
                    }
                },
                OpenIdCoreCertificates =
                [
                    new RPOpenIdCoreCertificate
                    {
                        Algorithm = "RS256",
                        Certificate = certificate,
                        KeyUsage = KeyUsageTypes.Signature
                    },
                    new RPOpenIdCoreCertificate
                    {
                        Algorithm = "RSA-OAEP-256",
                        Certificate = certificate,
                        KeyUsage = KeyUsageTypes.Encryption
                    },
                ],
                OpenIdFederationCertificates = new() { certificate },
                RequestedClaims = new()
                {
                    ClaimTypes.Name,
                    ClaimTypes.FamilyName,
                    ClaimTypes.Email,
                    ClaimTypes.FiscalNumber,
                    ClaimTypes.DateOfBirth,
                    ClaimTypes.PlaceOfBirth
                }
            });
            configure?.Invoke(o);
        };
    }

    public UrlEncoder Encoder => UrlEncoder.Default;

    public string ExpectedState { get; set; }

    public TestServer CreateTestServer(AuthenticationProperties properties = null, Func<HttpContext, Task> handler = null)
        => TestServerBuilder.CreateServer(_configureOptions, handler: handler, properties: properties);

    public IDictionary<string, string> ValidateChallengeRedirect(Uri redirectUri, params string[] parametersToValidate) =>
        ValidateRedirectCore(redirectUri, OpenIdConnectRequestType.Authentication, parametersToValidate);

    public IDictionary<string, string> ValidateSignoutRedirect(Uri redirectUri, params string[] parametersToValidate) =>
        ValidateRedirectCore(redirectUri, OpenIdConnectRequestType.Logout, parametersToValidate);

    private IDictionary<string, string> ValidateRedirectCore(Uri redirectUri, OpenIdConnectRequestType requestType, string[] parametersToValidate)
    {
        var errors = new List<string>();

        // Validate the authority
        ValidateExpectedAuthority(redirectUri.AbsoluteUri, errors, requestType);

        // Convert query to dictionary
        var queryDict = string.IsNullOrWhiteSpace(redirectUri.Query) ?
            new Dictionary<string, string>() :
            redirectUri.Query.TrimStart('?').Split('&').Select(part => part.Split('=')).ToDictionary(parts => parts[0], parts => parts[1]);

        // Validate the query string parameters
        ValidateParameters(queryDict, parametersToValidate, errors, htmlEncoded: true);

        if (errors.Any())
        {
            var buf = new StringBuilder();
            buf.AppendLine("The redirect uri is not valid.");
            buf.AppendLine(redirectUri.AbsoluteUri);

            foreach (var error in errors)
            {
                buf.AppendLine(error);
            }

            Debug.WriteLine(buf.ToString());
            Assert.True(false, buf.ToString());
        }

        return queryDict;
    }

    private void ValidateParameters(
        IDictionary<string, string> actualValues,
        IEnumerable<string> parametersToValidate,
        ICollection<string> errors,
        bool htmlEncoded)
    {
        foreach (var paramToValidate in parametersToValidate)
        {
            switch (paramToValidate)
            {
                case OpenIdConnectParameterNames.ClientId:
                    ValidateClientId(actualValues, errors, htmlEncoded);
                    break;
                case OpenIdConnectParameterNames.ResponseType:
                    ValidateResponseType(actualValues, errors, htmlEncoded);
                    break;
                case OpenIdConnectParameterNames.Scope:
                    ValidateScope(actualValues, errors, htmlEncoded);
                    break;
                case OpenIdConnectParameterNames.RedirectUri:
                    ValidateRedirectUri(actualValues, errors, htmlEncoded);
                    break;
                case OpenIdConnectParameterNames.Resource:
                    ValidateResource(actualValues, errors, htmlEncoded);
                    break;
                case OpenIdConnectParameterNames.State:
                    ValidateState(actualValues, errors, htmlEncoded);
                    break;
                case OpenIdConnectParameterNames.SkuTelemetry:
                    ValidateSkuTelemetry(actualValues, errors);
                    break;
                case OpenIdConnectParameterNames.VersionTelemetry:
                    ValidateVersionTelemetry(actualValues, errors, htmlEncoded);
                    break;
                case OpenIdConnectParameterNames.PostLogoutRedirectUri:
                    ValidatePostLogoutRedirectUri(actualValues, errors, htmlEncoded);
                    break;
                case OpenIdConnectParameterNames.Prompt:
                    ValidatePrompt(actualValues, errors, htmlEncoded);
                    break;
                default:
                    throw new InvalidOperationException($"Unknown parameter \"{paramToValidate}\".");
            }
        }
    }

    private void ValidateExpectedAuthority(string absoluteUri, ICollection<string> errors, OpenIdConnectRequestType requestType)
    {
        IEnumerable<string> expectedAuthorities;
        switch (requestType)
        {
            case OpenIdConnectRequestType.Token:
                expectedAuthorities = _options.SpidOPs.Select(s => s.EnsureTrailingSlash() + @"token")
                    .Concat(_options.CieOPs.Select(s => s.EnsureTrailingSlash() + @"token"));
                break;
            case OpenIdConnectRequestType.Logout:
                expectedAuthorities = _options.SpidOPs.Select(s => s.EnsureTrailingSlash() + @"revocation")
                    .Concat(_options.CieOPs.Select(s => s.EnsureTrailingSlash() + @"revocation"));
                break;
            default:
                expectedAuthorities = _options.SpidOPs.Select(s => s.EnsureTrailingSlash() + @"authorization")
                    .Concat(_options.CieOPs.Select(s => s.EnsureTrailingSlash() + @"authorization"));
                break;
        }

        if (!expectedAuthorities.Any(expectedAuthorities => absoluteUri.StartsWith(expectedAuthorities, StringComparison.Ordinal)))
        {
            errors.Add($"ExpectedAuthority: {expectedAuthorities}");
        }
    }

    private void ValidateClientId(IDictionary<string, string> actualParams, ICollection<string> errors, bool htmlEncoded) =>
        ValidateParameter(OpenIdConnectParameterNames.ClientId, _options.RelyingParties.FirstOrDefault().Id, actualParams, errors, htmlEncoded);

    private void ValidateResponseType(IDictionary<string, string> actualParams, ICollection<string> errors, bool htmlEncoded) =>
        ValidateParameter(OpenIdConnectParameterNames.ResponseType, SpidCieConst.ResponseType, actualParams, errors, htmlEncoded);

    private void ValidateScope(IDictionary<string, string> actualParams, ICollection<string> errors, bool htmlEncoded) =>
        ValidateParameter(OpenIdConnectParameterNames.Scope, string.Join(" ", new[] { SpidCieConst.OpenIdScope, SpidCieConst.OfflineScope }), actualParams, errors, htmlEncoded);

    private void ValidateRedirectUri(IDictionary<string, string> actualParams, ICollection<string> errors, bool htmlEncoded) =>
        ValidateParameter(OpenIdConnectParameterNames.RedirectUri, TestServerBuilder.TestHost + SpidCieConst.CallbackPath, actualParams, errors, htmlEncoded);

    private void ValidateResource(IDictionary<string, string> actualParams, ICollection<string> errors, bool htmlEncoded) =>
        ValidateParameter(OpenIdConnectParameterNames.RedirectUri, _options.RelyingParties.FirstOrDefault().Id, actualParams, errors, htmlEncoded);

    private void ValidateState(IDictionary<string, string> actualParams, ICollection<string> errors, bool htmlEncoded) =>
        ValidateParameter(OpenIdConnectParameterNames.State, ExpectedState, actualParams, errors, htmlEncoded);

    private static void ValidateSkuTelemetry(IDictionary<string, string> actualParams, ICollection<string> errors)
    {
        if (!actualParams.ContainsKey(OpenIdConnectParameterNames.SkuTelemetry))
        {
            errors.Add($"Parameter {OpenIdConnectParameterNames.SkuTelemetry} is missing");
        }
    }

    private void ValidateVersionTelemetry(IDictionary<string, string> actualParams, ICollection<string> errors, bool htmlEncoded) =>
        ValidateParameter(OpenIdConnectParameterNames.VersionTelemetry, typeof(OpenIdConnectMessage).GetTypeInfo().Assembly.GetName().Version.ToString(), actualParams, errors, htmlEncoded);

    private void ValidatePostLogoutRedirectUri(IDictionary<string, string> actualParams, ICollection<string> errors, bool htmlEncoded) =>
        ValidateParameter(OpenIdConnectParameterNames.PostLogoutRedirectUri, "https://example.com/signout-callback-oidc", actualParams, errors, htmlEncoded);

    private void ValidatePrompt(IDictionary<string, string> actualParams, ICollection<string> errors, bool htmlEncoded) =>
        ValidateParameter(OpenIdConnectParameterNames.Prompt, SpidCieConst.Prompt, actualParams, errors, htmlEncoded);

    private void ValidateParameter(
        string parameterName,
        string expectedValue,
        IDictionary<string, string> actualParams,
        ICollection<string> errors,
        bool htmlEncoded)
    {
        string actualValue;
        if (actualParams.TryGetValue(parameterName, out actualValue))
        {
            if (htmlEncoded)
            {
                expectedValue = Encoder.Encode(expectedValue);
            }

            if (actualValue != expectedValue)
            {
                errors.Add($"Parameter {parameterName}'s expected value is '{expectedValue}' but its actual value is '{actualValue}'");
            }
        }
        else
        {
            errors.Add($"Parameter {parameterName} is missing");
        }
    }
}