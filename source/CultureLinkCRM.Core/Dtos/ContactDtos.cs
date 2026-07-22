using CultureLinkCRM.Core.Entities;

namespace CultureLinkCRM.Core.Dtos;

/// <summary>Resolved contact info for a Person, after applying the Household fallback rule (per contact type independently).</summary>
public class EffectiveContactInfo
{
    public IReadOnlyList<PersonAddress> Addresses { get; init; } = [];
    public IReadOnlyList<PersonPhone> Phones { get; init; } = [];
    public IReadOnlyList<PersonEmail> Emails { get; init; } = [];
    public IReadOnlyList<HouseholdAddress> HouseholdAddresses { get; init; } = [];
    public IReadOnlyList<HouseholdPhone> HouseholdPhones { get; init; } = [];
    public IReadOnlyList<HouseholdEmail> HouseholdEmails { get; init; } = [];

    public bool AddressFromHousehold => Addresses.Count == 0 && HouseholdAddresses.Count > 0;
    public bool PhoneFromHousehold => Phones.Count == 0 && HouseholdPhones.Count > 0;
    public bool EmailFromHousehold => Emails.Count == 0 && HouseholdEmails.Count > 0;
}

public enum AudienceRowKind
{
    Household,
    Person,
    Organization
}

/// <summary>One de-duplicated row in an Audience's resolved membership (household-collapsed per FR-10).</summary>
public class AudienceMemberRow
{
    public required AudienceRowKind Kind { get; init; }
    public required string DisplayName { get; init; }
    public string? Street1 { get; init; }
    public string? City { get; init; }
    public string? StateProvince { get; init; }
    public string? PostalCode { get; init; }
    public string? Country { get; init; }
    public string? Phone { get; init; }
    public string? Email { get; init; }
    public int SourceId { get; init; }
}
