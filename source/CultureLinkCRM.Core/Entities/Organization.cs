using CultureLinkCRM.Core.Enums;

namespace CultureLinkCRM.Core.Entities;

public class Organization : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public OrganizationType OrganizationType { get; set; }

    public ICollection<OrganizationAddress> Addresses { get; set; } = [];
    public ICollection<OrganizationPhone> Phones { get; set; } = [];
    public ICollection<OrganizationEmail> Emails { get; set; } = [];

    public ICollection<PersonOrganization> PersonLinks { get; set; } = [];
    public ICollection<OrganizationNetwork> NetworkLinks { get; set; } = [];
    public ICollection<SegmentAssignment> SegmentAssignments { get; set; } = [];
}

public class OrganizationAddress
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public Organization? Organization { get; set; }
    public AddressType Type { get; set; }
    public bool IsPrimary { get; set; }
    public string Street1 { get; set; } = string.Empty;
    public string? Street2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string StateProvince { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
}

public class OrganizationPhone
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public Organization? Organization { get; set; }
    public PhoneType Type { get; set; }
    public bool IsPrimary { get; set; }
    public string Number { get; set; } = string.Empty;
}

public class OrganizationEmail
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public Organization? Organization { get; set; }
    public EmailType Type { get; set; }
    public bool IsPrimary { get; set; }
    public string Address { get; set; } = string.Empty;
}

public class PersonOrganization
{
    public int Id { get; set; }
    public int PersonId { get; set; }
    public Person? Person { get; set; }
    public int OrganizationId { get; set; }
    public Organization? Organization { get; set; }
    public string RoleTitle { get; set; } = string.Empty;
}
