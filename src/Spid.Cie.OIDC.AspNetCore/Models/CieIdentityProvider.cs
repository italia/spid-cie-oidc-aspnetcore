using System.Collections.Generic;
using System.Linq;

namespace Spid.Cie.OIDC.AspNetCore.Models;

internal sealed class CieIdentityProvider : IdentityProvider
{
    private static readonly Dictionary<string, string> _claimsMapping = new Dictionary<string, string> {
        {nameof(ClaimTypes.Name) , CieConst.given_name},
        {nameof(ClaimTypes.FamilyName) , CieConst.family_name},
        {nameof(ClaimTypes.FiscalNumber) , CieConst.fiscal_number},
        {nameof(ClaimTypes.Email) , CieConst.email},
        {nameof(ClaimTypes.Address) , CieConst.address},
        {nameof(ClaimTypes.DateOfBirth) , CieConst.birthdate},
        {nameof(ClaimTypes.Gender) , CieConst.gender},
        {nameof(ClaimTypes.PlaceOfBirth) , CieConst.place_of_birth},
        {nameof(ClaimTypes.PhoneNumber) , CieConst.phone_number},
        {nameof(ClaimTypes.DocumentDetails) , CieConst.document_details},
        {nameof(ClaimTypes.EmailVerified) , CieConst.email_verified},
        {nameof(ClaimTypes.IdANPR) , CieConst.idANPR},
        {nameof(ClaimTypes.EDeliveryService) , CieConst.e_delivery_service},
        {nameof(ClaimTypes.PhoneNumberVerified) , CieConst.phone_number_verified},
        {nameof(ClaimTypes.PhysicalPhoneNumber) , CieConst.physical_phone_number },
    };

    internal override IdentityProviderType Type { get => IdentityProviderType.CIE; }

    public override IEnumerable<string> FilterRequestedClaims(List<ClaimTypes> requestedClaims)
        => requestedClaims
            .Where(c => _claimsMapping.ContainsKey(c.Value))
            .Select(c => _claimsMapping[c.Value])
            .ToList()!;
}