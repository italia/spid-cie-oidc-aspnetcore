using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Spid.Cie.OIDC.AspNetCore.Models;

[ExcludeFromCodeCoverage]
public sealed class ClaimTypes
{
    private static readonly Dictionary<string, ClaimTypes> _types = new Dictionary<string, ClaimTypes>() {
        { nameof(Name), new ClaimTypes(nameof(Name)) },
        { nameof(FamilyName), new ClaimTypes(nameof(FamilyName)) },
        { nameof(FiscalNumber), new ClaimTypes(nameof(FiscalNumber)) },
        { nameof(Email), new ClaimTypes(nameof(Email)) },
        { nameof(EmailVerified), new ClaimTypes(nameof(EmailVerified)) },
        { nameof(DigitalAddress), new ClaimTypes(nameof(DigitalAddress)) },
        { nameof(Mail), new ClaimTypes(nameof(Mail)) },
        { nameof(Address), new ClaimTypes(nameof(Address)) },
        { nameof(CompanyName), new ClaimTypes(nameof(CompanyName)) },
        { nameof(CountyOfBirth), new ClaimTypes(nameof(CountyOfBirth)) },
        { nameof(DateOfBirth), new ClaimTypes(nameof(DateOfBirth)) },
        { nameof(ExpirationDate), new ClaimTypes(nameof(ExpirationDate)) },
        { nameof(Gender), new ClaimTypes(nameof(Gender)) },
        { nameof(IdCard), new ClaimTypes(nameof(IdCard)) },
        { nameof(IvaCode), new ClaimTypes(nameof(IvaCode)) },
        { nameof(PlaceOfBirth), new ClaimTypes(nameof(PlaceOfBirth)) },
        { nameof(RegisteredOffice), new ClaimTypes(nameof(RegisteredOffice)) },
        { nameof(SpidCode), new ClaimTypes(nameof(SpidCode)) },
        { nameof(CompanyFiscalNumber), new ClaimTypes(nameof(CompanyFiscalNumber)) },
        { nameof(DomicileStreetAddress), new ClaimTypes(nameof(DomicileStreetAddress)) },
        { nameof(DomicilePostalCode), new ClaimTypes(nameof(DomicilePostalCode)) },
        { nameof(DomicileMunicipality), new ClaimTypes(nameof(DomicileMunicipality)) },
        { nameof(DomicileProvince), new ClaimTypes(nameof(DomicileProvince)) },
        { nameof(DomicileNation), new ClaimTypes(nameof(DomicileNation)) },
        { nameof(DocumentDetails), new ClaimTypes(nameof(DocumentDetails)) },
        { nameof(IdANPR), new ClaimTypes(nameof(IdANPR)) },
        { nameof(EDeliveryService), new ClaimTypes(nameof(EDeliveryService)) },
        { nameof(PhoneNumber), new ClaimTypes(nameof(PhoneNumber)) },
        { nameof(PhoneNumberVerified), new ClaimTypes(nameof(PhoneNumberVerified)) },
        { nameof(PhysicalPhoneNumber), new ClaimTypes(nameof(PhysicalPhoneNumber)) }
    };

    private ClaimTypes(string value)
    {
        Value = value;
    }

    public static ClaimTypes FromName(string name)
    {
        return _types.ContainsKey(name) ? _types[name] : throw new System.Exception($"Invalid ClaimTypes name '{name}'");
    }

    public string Value { get; private set; }

    public static ClaimTypes Name { get { return _types[nameof(Name)]; } }
    public static ClaimTypes FamilyName { get { return _types[nameof(FamilyName)]; } }
    public static ClaimTypes FiscalNumber { get { return _types[nameof(FiscalNumber)]; } }
    public static ClaimTypes Email { get { return _types[nameof(Email)]; } }
    public static ClaimTypes DigitalAddress { get { return _types[nameof(DigitalAddress)]; } }
    public static ClaimTypes Mail { get { return _types[nameof(Mail)]; } }
    public static ClaimTypes Address { get { return _types[nameof(Address)]; } }
    public static ClaimTypes CompanyName { get { return _types[nameof(CompanyName)]; } }
    public static ClaimTypes CountyOfBirth { get { return _types[nameof(CountyOfBirth)]; } }
    public static ClaimTypes DateOfBirth { get { return _types[nameof(DateOfBirth)]; } }
    public static ClaimTypes ExpirationDate { get { return _types[nameof(ExpirationDate)]; } }
    public static ClaimTypes Gender { get { return _types[nameof(Gender)]; } }
    public static ClaimTypes IdCard { get { return _types[nameof(IdCard)]; } }
    public static ClaimTypes IvaCode { get { return _types[nameof(IvaCode)]; } }
    public static ClaimTypes PlaceOfBirth { get { return _types[nameof(PlaceOfBirth)]; } }
    public static ClaimTypes RegisteredOffice { get { return _types[nameof(RegisteredOffice)]; } }
    public static ClaimTypes SpidCode { get { return _types[nameof(SpidCode)]; } }
    public static ClaimTypes CompanyFiscalNumber { get { return _types[nameof(CompanyFiscalNumber)]; } }
    public static ClaimTypes DomicileStreetAddress { get { return _types[nameof(DomicileStreetAddress)]; } }
    public static ClaimTypes DomicilePostalCode { get { return _types[nameof(DomicilePostalCode)]; } }
    public static ClaimTypes DomicileMunicipality { get { return _types[nameof(DomicileMunicipality)]; } }
    public static ClaimTypes DomicileProvince { get { return _types[nameof(DomicileProvince)]; } }
    public static ClaimTypes DomicileNation { get { return _types[nameof(DomicileNation)]; } }
    public static ClaimTypes DocumentDetails { get { return _types[nameof(DocumentDetails)]; } }
    public static ClaimTypes EmailVerified { get { return _types[nameof(EmailVerified)]; } }
    public static ClaimTypes IdANPR { get { return _types[nameof(IdANPR)]; } }
    public static ClaimTypes EDeliveryService { get { return _types[nameof(EDeliveryService)]; } }
    public static ClaimTypes PhoneNumber { get { return _types[nameof(PhoneNumber)]; } }
    public static ClaimTypes PhoneNumberVerified { get { return _types[nameof(PhoneNumberVerified)]; } }
    public static ClaimTypes PhysicalPhoneNumber { get { return _types[nameof(PhysicalPhoneNumber)]; } }
}
