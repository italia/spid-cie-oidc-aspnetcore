using Microsoft.Extensions.Options;
using Spid.Cie.OIDC.AspNetCore.Configuration;
using Spid.Cie.OIDC.AspNetCore.Enums;
using Spid.Cie.OIDC.AspNetCore.Models;
using System;
using System.Security.Cryptography.X509Certificates;

namespace Spid.Cie.OIDC.AspNetCore.Tests.Mocks;

internal class MockOptionsMonitorSpidCieOptions : IOptionsMonitor<SpidCieOptions>
{
    private readonly bool _emptyCollection;
    private readonly bool _noKeys;

    public MockOptionsMonitorSpidCieOptions(bool emptyCollection = false, bool noKeys = false)
    {
        _emptyCollection = emptyCollection;
        _noKeys = noKeys;
    }

    public SpidCieOptions CurrentValue => Get(Options.DefaultName);

    public SpidCieOptions Get(string name)
    {
        var options = new SpidCieOptions();
        if (!_emptyCollection)
        {
            var certificate = new X509Certificate2("ComuneVigata-SPID.pfx", "P@ssW0rd!");
            options.SpidOPs.Add("http://127.0.0.1:8000/oidc/op/");
            options.CieOPs.Add("http://127.0.0.1:8002/oidc/op/");
            options.RelyingParties.Add(new RelyingParty()
            {
                Id = "http://127.0.0.1:5000/",
                Name = "RP Test",
                Contacts = new() { "info@rptest.it" },
                AuthorityHints = new() { "http://127.0.0.1:8000/" },
                RedirectUris = new() { "http://127.0.0.1:5000/signin-oidc" },
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
                OpenIdCoreCertificates = _noKeys ? new() : new()
                {
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
                    }
                },
                OpenIdFederationCertificates = _noKeys ? new() : new() { certificate },
            }); ;
        }
        return options;
    }

    public IDisposable OnChange(Action<SpidCieOptions, string> listener)
    {
        throw new NotImplementedException();
    }
}
