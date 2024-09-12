using System.Diagnostics.CodeAnalysis;

namespace Spid.Cie.OIDC.AspNetCore;

[ExcludeFromCodeCoverage]
class CieConst
{
    internal const string email = nameof(email);
    internal const string gender = nameof(gender);
    internal const string address = nameof(address);
    internal const string birthdate = nameof(birthdate);
    internal const string given_name = nameof(given_name);
    internal const string family_name = nameof(family_name);
    internal const string phone_number = nameof(phone_number);
    internal const string place_of_birth = nameof(place_of_birth);
    internal const string email_verified = nameof(email_verified);
    internal const string document_details = nameof(document_details);
    internal const string idANPR = $"{AttributesBaseURI}{nameof(idANPR)}";
    internal const string AttributesBaseURI = "https://attributes.eid.gov.it/";
    internal const string phone_number_verified = nameof(phone_number_verified);
    internal const string fiscal_number = $"{AttributesBaseURI}{nameof(fiscal_number)}";
    internal const string e_delivery_service = $"{AttributesBaseURI}{nameof(e_delivery_service)}";
    internal const string physical_phone_number = $"{AttributesBaseURI}{nameof(physical_phone_number)}";
}