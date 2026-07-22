namespace CultureLinkCRM.Core.Entities;

public class Person : AuditableEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string? Suffix { get; set; }

    public int? HouseholdId { get; set; }
    public Household? Household { get; set; }

    public ICollection<PersonAddress> Addresses { get; set; } = [];
    public ICollection<PersonPhone> Phones { get; set; } = [];
    public ICollection<PersonEmail> Emails { get; set; } = [];

    public ICollection<PersonOrganization> OrganizationLinks { get; set; } = [];
    public ICollection<PersonNetwork> NetworkLinks { get; set; } = [];
    public ICollection<SegmentAssignment> SegmentAssignments { get; set; } = [];

    public string FullName => string.Join(' ', new[] { FirstName, MiddleName, LastName, Suffix }
        .Where(part => !string.IsNullOrWhiteSpace(part)));
}
