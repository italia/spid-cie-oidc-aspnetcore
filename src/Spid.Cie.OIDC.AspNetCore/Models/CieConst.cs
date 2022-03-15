namespace Spid.Cie.OIDC.AspNetCore.Models;

internal class CieConst
{
    internal const string Cie_L1 = nameof(Cie_L1);
    internal const string Cie_L2 = nameof(Cie_L2);
    internal const string Cie_L3 = nameof(Cie_L3);
    internal const string DefaultAcr = Cie_L2;

    internal const string given_name = nameof(given_name);
    internal const string family_name = nameof(family_name);
    internal const string email = nameof(email);
    internal const string email_verified = nameof(email_verified);
    internal const string gender = nameof(gender);
    internal const string birthdate = nameof(birthdate);
    internal const string phone_number = nameof(phone_number);
    internal const string phone_number_verified = nameof(phone_number_verified);
    internal const string address = nameof(address);
    internal const string place_of_birth = nameof(place_of_birth);
    internal const string document_details = nameof(document_details);

    internal const string AttributesBaseURI = "https://idserver.servizicie.interno.gov.it/";

    internal const string e_delivery_service = $"{AttributesBaseURI}{nameof(e_delivery_service)}";
    internal const string fiscal_number = $"{AttributesBaseURI}{nameof(fiscal_number)}";
    internal const string idANPR = $"{AttributesBaseURI}{nameof(idANPR)}";
    internal const string physical_phone_number = $"{AttributesBaseURI}{nameof(physical_phone_number)}";
}
