using CultureLinkCRM.Core.Enums;

namespace CultureLinkCRM.Core.Entities;

public class Household : AuditableEntity
{
    public string HouseholdName { get; set; } = string.Empty;
    public MailPreference MailPreference { get; set; } = MailPreference.MailToHousehold;

    public ICollection<HouseholdAddress> Addresses { get; set; } = [];
    public ICollection<HouseholdPhone> Phones { get; set; } = [];
    public ICollection<HouseholdEmail> Emails { get; set; } = [];

    public ICollection<Person> Members { get; set; } = [];
}

public class HouseholdAddress
{
    public int Id { get; set; }
    public int HouseholdId { get; set; }
    public Household? Household { get; set; }
    public AddressType Type { get; set; }
    public bool IsPrimary { get; set; }
    public string Street1 { get; set; } = string.Empty;
    public string? Street2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string StateProvince { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
}

public class HouseholdPhone
{
    public int Id { get; set; }
    public int HouseholdId { get; set; }
    public Household? Household { get; set; }
    public PhoneType Type { get; set; }
    public bool IsPrimary { get; set; }
    public string Number { get; set; } = string.Empty;
}

public class HouseholdEmail
{
    public int Id { get; set; }
    public int HouseholdId { get; set; }
    public Household? Household { get; set; }
    public EmailType Type { get; set; }
    public bool IsPrimary { get; set; }
    public string Address { get; set; } = string.Empty;
}
