using Spid.Cie.OIDC.AspNetCore.Enums;
using Spid.Cie.OIDC.AspNetCore.Models;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Spid.Cie.OIDC.AspNetCore.Tests;

public class SpidIdentityProvidersTests
{
    [Fact]
    public async Task TestFilterRequestedClaims()
    {
        await Task.CompletedTask;

        var idp = new SpidIdentityProvider()
        {
            EntityConfiguration = new OPEntityConfiguration()
            {
                Metadata = new OPMetadata_SpidCieOIDCConfiguration()
                {
                    OpenIdProvider = new Microsoft.IdentityModel.Protocols.OpenIdConnect.OpenIdConnectConfiguration()
                }
            }
        };

        idp.EntityConfiguration.Metadata.OpenIdProvider.ClaimsSupported.Add(SpidConst.given_name);
        idp.EntityConfiguration.Metadata.OpenIdProvider.ClaimsSupported.Add(SpidConst.family_name);


        var filteredClaims = idp.FilterRequestedClaims(new()
        {
            ClaimTypes.Name,
            ClaimTypes.FamilyName,
            ClaimTypes.FiscalNumber,
            ClaimTypes.Email,
            ClaimTypes.DigitalAddress,
            ClaimTypes.Mail,
            ClaimTypes.Address,
            ClaimTypes.CompanyName,
            ClaimTypes.CountyOfBirth,
            ClaimTypes.DateOfBirth,
            ClaimTypes.ExpirationDate,
            ClaimTypes.Gender,
            ClaimTypes.IdCard,
            ClaimTypes.IvaCode,
            ClaimTypes.PlaceOfBirth,
            ClaimTypes.RegisteredOffice,
            ClaimTypes.SpidCode,
            ClaimTypes.CompanyFiscalNumber,
            ClaimTypes.DocumentDetails,
            ClaimTypes.EmailVerified,
            ClaimTypes.IdANPR,
            ClaimTypes.EDeliveryService,
            ClaimTypes.PhoneNumber,
            ClaimTypes.PhoneNumberVerified,
            ClaimTypes.PhysicalPhoneNumber,
        });

        Assert.True(filteredClaims.Contains(SpidConst.given_name));
    }

    [Fact]
    public async Task TestGetAcrValue()
    {
        await Task.CompletedTask;

        var idp = new SpidIdentityProvider()
        {
            EntityConfiguration = new OPEntityConfiguration()
            {
                Metadata = new OPMetadata_SpidCieOIDCConfiguration()
                {
                    OpenIdProvider = new Microsoft.IdentityModel.Protocols.OpenIdConnect.OpenIdConnectConfiguration()
                }
            }
        };

        idp.SupportedAcrValues = new() { SpidCieConst.SpidL2, SpidCieConst.SpidL1, SpidCieConst.SpidL3 };

        var acr = idp.GetAcrValue(SecurityLevels.L2);

        Assert.Contains(SpidCieConst.SpidL2, acr);

        acr = idp.GetAcrValue(SecurityLevels.L1);

        Assert.Contains(SpidCieConst.SpidL1, acr);

        acr = idp.GetAcrValue(SecurityLevels.L3);

        Assert.Contains(SpidCieConst.SpidL3, acr);
    }
}
