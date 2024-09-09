using System.Diagnostics.CodeAnalysis;

namespace Spid.Cie.OIDC.AspNetCore;

[ExcludeFromCodeCoverage]
internal class SpidConst
{
    internal const string email = nameof(email);
    internal const string gender = nameof(gender);
    internal const string birthdate = nameof(birthdate);
    internal const string given_name = nameof(given_name);
    internal const string family_name = nameof(family_name);
    internal const string phone_number = nameof(phone_number);
    internal const string place_of_birth = nameof(place_of_birth);
    internal const string mail = $"{AttributesBaseURI}{nameof(mail)}";
    internal const string document_details = nameof(document_details);
    internal const string idCard = $"{AttributesBaseURI}{nameof(idCard)}";
    internal const string address = $"{AttributesBaseURI}{nameof(address)}";
    internal const string AttributesBaseURI = "https://attributes.eid.gov.it";
    internal const string spid_code = $"{AttributesBaseURI}{nameof(spid_code)}";
    internal const string vat_number = $"{AttributesBaseURI}{nameof(vat_number)}";
    internal const string company_name = $"{AttributesBaseURI}{nameof(company_name)}";
    internal const string eid_exp_date = $"{AttributesBaseURI}{nameof(eid_exp_date)}";
    internal const string fiscal_number = $"{AttributesBaseURI}{nameof(fiscal_number)}";
    internal const string countyOfBirth = $"{AttributesBaseURI}{nameof(countyOfBirth)}";
    internal const string registered_office = $"{AttributesBaseURI}{nameof(registered_office)}";
    internal const string e_delivery_service = $"{AttributesBaseURI}{nameof(e_delivery_service)}";
    internal const string company_fiscal_number = $"{AttributesBaseURI}{nameof(company_fiscal_number)}";
}