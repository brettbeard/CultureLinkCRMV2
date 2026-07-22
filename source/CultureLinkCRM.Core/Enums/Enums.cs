namespace CultureLinkCRM.Core.Enums;

public enum AddressType
{
    Home,
    Work,
    Other
}

public enum PhoneType
{
    Home,
    Work,
    Mobile,
    Other
}

public enum EmailType
{
    Home,
    Work,
    Other
}

public enum MailPreference
{
    MailToHousehold,
    MailToIndividual,
    DoNotMail
}

public enum OrganizationType
{
    Church,
    Ministry,
    Org,
    Business,
    Foundation
}

public enum NetworkType
{
    Agency,
    DenominationAssociation,
    Region,
    MinistryFocus,
    NetworkingGroup,
    ConferenceConnection
}

public enum UserRole
{
    Admin,
    Superuser,
    User
}

public enum DonorStatus
{
    NoDonationHistory,
    Active,
    Lapsed
}
