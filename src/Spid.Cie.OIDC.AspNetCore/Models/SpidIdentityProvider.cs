using System.Collections.Generic;
using System.Linq;

namespace Spid.Cie.OIDC.AspNetCore.Models;

internal sealed class SpidIdentityProvider : IdentityProvider
{
    private static readonly Dictionary<string, string> _claimsMapping = new Dictionary<string, string> {
        {nameof(ClaimTypes.Name) , SpidConst.name},
        {nameof(ClaimTypes.FamilyName) , SpidConst.familyName},
        {nameof(ClaimTypes.FiscalNumber) , SpidConst.fiscalNumber},
        {nameof(ClaimTypes.Email) , SpidConst.email},
        {nameof(ClaimTypes.DigitalAddress) , SpidConst.digitalAddress},
        {nameof(ClaimTypes.Mail) , SpidConst.mail},
        {nameof(ClaimTypes.Address) , SpidConst.address},
        {nameof(ClaimTypes.CompanyName) , SpidConst.companyName},
        {nameof(ClaimTypes.CountyOfBirth) , SpidConst.countyOfBirth},
        {nameof(ClaimTypes.DateOfBirth) , SpidConst.dateOfBirth},
        {nameof(ClaimTypes.ExpirationDate) , SpidConst.expirationDate},
        {nameof(ClaimTypes.Gender) , SpidConst.gender},
        {nameof(ClaimTypes.IdCard) , SpidConst.idCard},
        {nameof(ClaimTypes.IvaCode) , SpidConst.ivaCode},
        {nameof(ClaimTypes.PlaceOfBirth) , SpidConst.placeOfBirth},
        {nameof(ClaimTypes.RegisteredOffice) , SpidConst.registeredOffice},
        {nameof(ClaimTypes.SpidCode) , SpidConst.spidCode},
        {nameof(ClaimTypes.CompanyFiscalNumber) , SpidConst.companyFiscalNumber},
        {nameof(ClaimTypes.DomicileStreetAddress) , SpidConst.domicileStreetAddress},
        {nameof(ClaimTypes.DomicilePostalCode) , SpidConst.domicilePostalCode},
        {nameof(ClaimTypes.DomicileMunicipality) , SpidConst.domicileMunicipality},
        {nameof(ClaimTypes.DomicileProvince) , SpidConst.domicileProvince},
        {nameof(ClaimTypes.DomicileNation) , SpidConst.domicileNation},
        {nameof(ClaimTypes.PhoneNumber) , SpidConst.mobilePhone},
    };

    internal override IdentityProviderType Type { get => IdentityProviderType.SPID; }

    public override IEnumerable<string> FilterRequestedClaims(List<ClaimTypes> requestedClaims)
        => requestedClaims
            .Where(c => _claimsMapping.ContainsKey(c.Value))
            .Select(c => _claimsMapping[c.Value])
            .ToList()!;

    public override string GetAcrValue(SecurityLevel securityLevel)
    {
        return securityLevel == SecurityLevel.L1 && base.SupportedAcrValues.Contains(SpidConst.SpidL1)
            ? SpidConst.SpidL1
            : securityLevel == SecurityLevel.L2 && base.SupportedAcrValues.Contains(SpidConst.SpidL2)
            ? SpidConst.SpidL2
            : securityLevel == SecurityLevel.L3 && base.SupportedAcrValues.Contains(SpidConst.SpidL3)
            ? SpidConst.SpidL3
            : SpidConst.DefaultAcr;
    }
}
