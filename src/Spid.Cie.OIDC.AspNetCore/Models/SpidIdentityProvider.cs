using Spid.Cie.OIDC.AspNetCore.Enums;
using System.Collections.Generic;
using System.Linq;

namespace Spid.Cie.OIDC.AspNetCore.Models;

sealed class SpidIdentityProvider : IdentityProvider
{
    private static readonly Dictionary<string, string> _claimsMapping = new Dictionary<string, string> {
        {nameof(ClaimTypes.Name) , SpidConst.given_name},
        {nameof(ClaimTypes.FamilyName) , SpidConst.family_name},
        {nameof(ClaimTypes.FiscalNumber) , SpidConst.fiscal_number},
        {nameof(ClaimTypes.Email) , SpidConst.email},
        {nameof(ClaimTypes.DigitalAddress) , SpidConst.e_delivery_service},
        {nameof(ClaimTypes.Mail) , SpidConst.mail},
        {nameof(ClaimTypes.Address) , SpidConst.address},
        {nameof(ClaimTypes.CompanyName) , SpidConst.company_name},
        {nameof(ClaimTypes.CountyOfBirth) , SpidConst.countyOfBirth},
        {nameof(ClaimTypes.DateOfBirth) , SpidConst.birthdate},
        {nameof(ClaimTypes.ExpirationDate) , SpidConst.eid_exp_date},
        {nameof(ClaimTypes.Gender) , SpidConst.gender},
        {nameof(ClaimTypes.IdCard) , SpidConst.idCard},
        {nameof(ClaimTypes.IvaCode) , SpidConst.vat_number},
        {nameof(ClaimTypes.PlaceOfBirth) , SpidConst.place_of_birth},
        {nameof(ClaimTypes.DocumentDetails) , SpidConst.document_details},
        {nameof(ClaimTypes.RegisteredOffice) , SpidConst.registered_office},
        {nameof(ClaimTypes.SpidCode) , SpidConst.spid_code},
        {nameof(ClaimTypes.CompanyFiscalNumber) , SpidConst.company_fiscal_number},
        {nameof(ClaimTypes.PhoneNumber) , SpidConst.phone_number},
    };

    internal override IdentityProviderTypes Type { get => IdentityProviderTypes.SPID; }

    public override IEnumerable<string> FilterRequestedClaims(List<ClaimTypes> requestedClaims)
        => requestedClaims
            .Where(c => _claimsMapping.ContainsKey(c.Value))
            .Select(c => _claimsMapping[c.Value])
            .ToList()!;
}