using System;
using System.Collections.Generic;

namespace Spid.Cie.OIDC.AspNetCore.Models;

public sealed class CieClaimTypes
{
    private static readonly Dictionary<string, CieClaimTypes> _types = new Dictionary<string, CieClaimTypes>() {
        { nameof(FamilyName), new CieClaimTypes(nameof(FamilyName)) },
        { nameof(FiscalNumber), new CieClaimTypes(nameof(FiscalNumber)) },
        { nameof(RawFiscalNumber), new CieClaimTypes(nameof(RawFiscalNumber)) },
        { nameof(Email), new CieClaimTypes(nameof(Email)) },
        { nameof(DocumentDetails), new CieClaimTypes(nameof(DocumentDetails)) },
        { nameof(Address), new CieClaimTypes(nameof(Address)) },
        { nameof(EmailVerified), new CieClaimTypes(nameof(EmailVerified)) },
        { nameof(BirthDate), new CieClaimTypes(nameof(BirthDate)) },
        { nameof(IdANPR), new CieClaimTypes(nameof(IdANPR)) },
        { nameof(Gender), new CieClaimTypes(nameof(Gender)) },
        { nameof(GivenName), new CieClaimTypes(nameof(GivenName)) },
        { nameof(EDeliveryService), new CieClaimTypes(nameof(EDeliveryService)) },
        { nameof(PhoneNumber), new CieClaimTypes(nameof(PhoneNumber)) },
        { nameof(PlaceOfBirth), new CieClaimTypes(nameof(PlaceOfBirth)) },
        { nameof(PhoneNumberVerified), new CieClaimTypes(nameof(PhoneNumberVerified)) },
        { nameof(PhysicalPhoneNumber), new CieClaimTypes(nameof(PhysicalPhoneNumber)) }
    };

    private CieClaimTypes(string value)
    {
        Value = value;
    }

    public string Value { get; private set; }

    public static CieClaimTypes FamilyName { get { return _types[nameof(FamilyName)]; } }
    public static CieClaimTypes FiscalNumber { get { return _types[nameof(FiscalNumber)]; } }
    public static CieClaimTypes RawFiscalNumber { get { return _types[nameof(RawFiscalNumber)]; } }
    public static CieClaimTypes Email { get { return _types[nameof(Email)]; } }
    public static CieClaimTypes DocumentDetails { get { return _types[nameof(DocumentDetails)]; } }
    public static CieClaimTypes Address { get { return _types[nameof(Address)]; } }
    public static CieClaimTypes EmailVerified { get { return _types[nameof(EmailVerified)]; } }
    public static CieClaimTypes BirthDate { get { return _types[nameof(BirthDate)]; } }
    public static CieClaimTypes IdANPR { get { return _types[nameof(IdANPR)]; } }
    public static CieClaimTypes Gender { get { return _types[nameof(Gender)]; } }
    public static CieClaimTypes GivenName { get { return _types[nameof(GivenName)]; } }
    public static CieClaimTypes EDeliveryService { get { return _types[nameof(EDeliveryService)]; } }
    public static CieClaimTypes PhoneNumber { get { return _types[nameof(PhoneNumber)]; } }
    public static CieClaimTypes PlaceOfBirth { get { return _types[nameof(PlaceOfBirth)]; } }
    public static CieClaimTypes PhoneNumberVerified { get { return _types[nameof(PhoneNumberVerified)]; } }
    public static CieClaimTypes PhysicalPhoneNumber { get { return _types[nameof(PhysicalPhoneNumber)]; } }

    internal string GetOIDCClaimName()
    {
        return Value switch
        {
            nameof(Address) => CieConst.address,
            nameof(FamilyName) => CieConst.family_name,
            nameof(FiscalNumber) or nameof(RawFiscalNumber) => CieConst.fiscal_number,
            nameof(Email) => CieConst.email,
            nameof(DocumentDetails) => CieConst.document_details,
            nameof(EmailVerified) => CieConst.email_verified,
            nameof(GivenName) => CieConst.given_name,
            nameof(BirthDate) => CieConst.birthdate,
            nameof(EDeliveryService) => CieConst.e_delivery_service,
            nameof(Gender) => CieConst.gender,
            nameof(IdANPR) => CieConst.idANPR,
            nameof(PhoneNumber) => CieConst.phone_number,
            nameof(PhoneNumberVerified) => CieConst.phone_number_verified,
            nameof(PhysicalPhoneNumber) => CieConst.physical_phone_number,
            nameof(PlaceOfBirth) => CieConst.place_of_birth,
            _ => throw new Exception("Invalid ClaimType"),
        };
    }

}
