using Spid.Cie.OIDC.AspNetCore.Models;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Spid.Cie.OIDC.AspNetCore.Tests;

public class CieIdentityProvidersTests
{

    [Fact]
    public async Task TestFilterRequestedClaims()
    {
        await Task.CompletedTask;

        var idp = new CieIdentityProvider()
        {
            EntityConfiguration = new IdPEntityConfiguration()
            {
                Metadata = new IdPMetadata_SpidCieOIDCConfiguration()
                {
                    OpenIdProvider = new Microsoft.IdentityModel.Protocols.OpenIdConnect.OpenIdConnectConfiguration()
                }
            }
        };

        idp.EntityConfiguration.Metadata.OpenIdProvider.ClaimsSupported.Add(CieConst.given_name);
        idp.EntityConfiguration.Metadata.OpenIdProvider.ClaimsSupported.Add(CieConst.family_name);


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
            ClaimTypes.DomicileStreetAddress,
            ClaimTypes.DomicilePostalCode,
            ClaimTypes.DomicileMunicipality,
            ClaimTypes.DomicileProvince,
            ClaimTypes.DomicileNation,
            ClaimTypes.DocumentDetails,
            ClaimTypes.EmailVerified,
            ClaimTypes.IdANPR,
            ClaimTypes.EDeliveryService,
            ClaimTypes.PhoneNumber,
            ClaimTypes.PhoneNumberVerified,
            ClaimTypes.PhysicalPhoneNumber,

        });

        Assert.True(filteredClaims.Contains(CieConst.given_name));
        Assert.True(filteredClaims.Contains(CieConst.family_name));
    }

    [Fact]
    public async Task TestGetAcrValue()
    {
        await Task.CompletedTask;

        var idp = new CieIdentityProvider()
        {
            EntityConfiguration = new IdPEntityConfiguration()
            {
                Metadata = new IdPMetadata_SpidCieOIDCConfiguration()
                {
                    OpenIdProvider = new Microsoft.IdentityModel.Protocols.OpenIdConnect.OpenIdConnectConfiguration()
                }
            }
        };

        idp.SupportedAcrValues = new() { CieConst.Cie_L1, CieConst.Cie_L2, CieConst.Cie_L3 };

        var acr = idp.GetAcrValue(SecurityLevel.L2);

        Assert.True(acr.Contains(CieConst.Cie_L2));

        acr = idp.GetAcrValue(SecurityLevel.L1);

        Assert.True(acr.Contains(CieConst.Cie_L1));

        acr = idp.GetAcrValue(SecurityLevel.L3);

        Assert.True(acr.Contains(CieConst.Cie_L3));
    }
}
