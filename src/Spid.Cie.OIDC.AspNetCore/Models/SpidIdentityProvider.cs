using System;
using System.Collections.Generic;
using System.Linq;

namespace Spid.Cie.OIDC.AspNetCore.Models;

internal sealed class SpidIdentityProvider : IdentityProvider
{
    internal SpidIdentityProvider()
    {

    }

    internal override IdentityProviderType Type { get => IdentityProviderType.SPID; }

    public override IEnumerable<string> FilterRequestedClaims(ClaimTypes[] requestedClaims)
    {
        List<string> claims = new();
        foreach (var requestedClaim in requestedClaims)
        {
            var mappedClaim = requestedClaim.Value switch
            {
                nameof(ClaimTypes.Name) => SpidConst.name,
                nameof(ClaimTypes.FamilyName) => SpidConst.familyName,
                nameof(ClaimTypes.FiscalNumber) => SpidConst.fiscalNumber,
                nameof(ClaimTypes.Email) => SpidConst.email,
                nameof(ClaimTypes.DigitalAddress) => SpidConst.digitalAddress,
                nameof(ClaimTypes.Mail) => SpidConst.mail,
                nameof(ClaimTypes.Address) => SpidConst.address,
                nameof(ClaimTypes.CompanyName) => SpidConst.companyName,
                nameof(ClaimTypes.CountyOfBirth) => SpidConst.countyOfBirth,
                nameof(ClaimTypes.DateOfBirth) => SpidConst.dateOfBirth,
                nameof(ClaimTypes.ExpirationDate) => SpidConst.expirationDate,
                nameof(ClaimTypes.Gender) => SpidConst.gender,
                nameof(ClaimTypes.IdCard) => SpidConst.idCard,
                nameof(ClaimTypes.IvaCode) => SpidConst.ivaCode,
                nameof(ClaimTypes.PlaceOfBirth) => SpidConst.placeOfBirth,
                nameof(ClaimTypes.RegisteredOffice) => SpidConst.registeredOffice,
                nameof(ClaimTypes.SpidCode) => SpidConst.spidCode,
                nameof(ClaimTypes.CompanyFiscalNumber) => SpidConst.companyFiscalNumber,
                nameof(ClaimTypes.DomicileStreetAddress) => SpidConst.domicileStreetAddress,
                nameof(ClaimTypes.DomicilePostalCode) => SpidConst.domicilePostalCode,
                nameof(ClaimTypes.DomicileMunicipality) => SpidConst.domicileMunicipality,
                nameof(ClaimTypes.DomicileProvince) => SpidConst.domicileProvince,
                nameof(ClaimTypes.DomicileNation) => SpidConst.domicileNation,
                nameof(ClaimTypes.PhoneNumber) => SpidConst.mobilePhone,
                _ => null,
            };
            if (!string.IsNullOrWhiteSpace(mappedClaim))
                claims.Add(mappedClaim);
        }
        return claims;
    }

    public override string GetAcrValue(SecurityLevel securityLevel)
        => securityLevel switch
        {
            Models.SecurityLevel.L1 => base.SupportedAcrValues.Contains(SpidConst.SpidL1) ? SpidConst.SpidL1 : SpidConst.DefaultAcr,
            Models.SecurityLevel.L3 => base.SupportedAcrValues.Contains(SpidConst.SpidL3) ? SpidConst.SpidL3 : SpidConst.DefaultAcr,
            _ => base.SupportedAcrValues.Contains(SpidConst.SpidL2) ? SpidConst.SpidL2 : SpidConst.DefaultAcr,
        };
}
