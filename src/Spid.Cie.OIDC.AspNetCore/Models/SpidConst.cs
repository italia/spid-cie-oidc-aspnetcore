using System.Diagnostics.CodeAnalysis;

namespace Spid.Cie.OIDC.AspNetCore.Models;

[ExcludeFromCodeCoverage]
internal class SpidConst
{
    internal const string SpidLevelBaseURI = "https://www.spid.gov.it/";

    internal const string SpidL1 = $"{SpidLevelBaseURI}{nameof(SpidL1)}";
    internal const string SpidL2 = $"{SpidLevelBaseURI}{nameof(SpidL2)}";
    internal const string SpidL3 = $"{SpidLevelBaseURI}{nameof(SpidL3)}";
    internal const string DefaultAcr = SpidL2;

    internal const string AttributesBaseURI = "https://attributes.spid.gov.it/";

    internal const string name = $"{AttributesBaseURI}{nameof(name)}";
    internal const string familyName = $"{AttributesBaseURI}{nameof(familyName)}";
    internal const string fiscalNumber = $"{AttributesBaseURI}{nameof(fiscalNumber)}";
    internal const string email = $"{AttributesBaseURI}{nameof(email)}";
    internal const string digitalAddress = $"{AttributesBaseURI}{nameof(digitalAddress)}";
    internal const string mail = $"{AttributesBaseURI}{nameof(mail)}";
    internal const string address = $"{AttributesBaseURI}{nameof(address)}";
    internal const string companyName = $"{AttributesBaseURI}{nameof(companyName)}";
    internal const string countyOfBirth = $"{AttributesBaseURI}{nameof(countyOfBirth)}";
    internal const string dateOfBirth = $"{AttributesBaseURI}{nameof(dateOfBirth)}";
    internal const string expirationDate = $"{AttributesBaseURI}{nameof(expirationDate)}";
    internal const string gender = $"{AttributesBaseURI}{nameof(gender)}";
    internal const string idCard = $"{AttributesBaseURI}{nameof(idCard)}";
    internal const string ivaCode = $"{AttributesBaseURI}{nameof(ivaCode)}";
    internal const string mobilePhone = $"{AttributesBaseURI}{nameof(mobilePhone)}";
    internal const string placeOfBirth = $"{AttributesBaseURI}{nameof(placeOfBirth)}";
    internal const string registeredOffice = $"{AttributesBaseURI}{nameof(registeredOffice)}";
    internal const string spidCode = $"{AttributesBaseURI}{nameof(spidCode)}";

    internal const string companyFiscalNumber = $"{AttributesBaseURI}{nameof(companyFiscalNumber)}";
    internal const string domicileStreetAddress = $"{AttributesBaseURI}{nameof(domicileStreetAddress)}";
    internal const string domicilePostalCode = $"{AttributesBaseURI}{nameof(domicilePostalCode)}";
    internal const string domicileMunicipality = $"{AttributesBaseURI}{nameof(domicileMunicipality)}";
    internal const string domicileProvince = $"{AttributesBaseURI}{nameof(domicileProvince)}";
    internal const string domicileNation = $"{AttributesBaseURI}{nameof(domicileNation)}";
}
