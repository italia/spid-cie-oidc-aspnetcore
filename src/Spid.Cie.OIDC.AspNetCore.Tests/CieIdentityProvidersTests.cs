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
            EntityConfiguration = new OPEntityConfiguration()
            {
                Metadata = new OPMetadata_SpidCieOIDCConfiguration()
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


}
