using Spid.Cie.OIDC.AspNetCore.Enums;
using System.Collections.Generic;
using System.Linq;

namespace Spid.Cie.OIDC.AspNetCore.Models;

sealed class CieIdentityProvider : IdentityProvider
{
    static readonly Dictionary<string, string> _claimsMapping = new()
    {
        {nameof(ClaimTypes.Email) , CieConst.email},
        {nameof(ClaimTypes.Gender) , CieConst.gender},
        {nameof(ClaimTypes.IdANPR) , CieConst.idANPR},
        {nameof(ClaimTypes.Name) , CieConst.given_name},
        {nameof(ClaimTypes.Address) , CieConst.address},
        {nameof(ClaimTypes.DateOfBirth) , CieConst.birthdate},
        {nameof(ClaimTypes.FamilyName) , CieConst.family_name},
        {nameof(ClaimTypes.PhoneNumber) , CieConst.phone_number},
        {nameof(ClaimTypes.FiscalNumber) , CieConst.fiscal_number},
        {nameof(ClaimTypes.PlaceOfBirth) , CieConst.place_of_birth},
        {nameof(ClaimTypes.EmailVerified) , CieConst.email_verified},
        {nameof(ClaimTypes.DocumentDetails) , CieConst.document_details},
        {nameof(ClaimTypes.EDeliveryService) , CieConst.e_delivery_service},
        {nameof(ClaimTypes.PhoneNumberVerified) , CieConst.phone_number_verified},
        {nameof(ClaimTypes.PhysicalPhoneNumber) , CieConst.physical_phone_number }
    };

    internal override IdentityProviderTypes Type { get => IdentityProviderTypes.CIE; }

    public override IEnumerable<string> FilterRequestedClaims(List<ClaimTypes> requestedClaims)
        => requestedClaims
            .Where(c => _claimsMapping.ContainsKey(c.Value))
            .Select(c => _claimsMapping[c.Value])
            .ToList()!;
}