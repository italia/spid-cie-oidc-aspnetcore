using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Spid.Cie.OIDC.AspNetCore.Tests.Mocks;

internal partial class TestSettings
{
    internal class MockProtocolValidator : OpenIdConnectProtocolValidator
    {
        public override void ValidateAuthenticationResponse(OpenIdConnectProtocolValidationContext validationContext)
        {
        }

        public override void ValidateTokenResponse(OpenIdConnectProtocolValidationContext validationContext)
        {
        }

        public override void ValidateUserInfoResponse(OpenIdConnectProtocolValidationContext validationContext)
        {
        }
    }
}