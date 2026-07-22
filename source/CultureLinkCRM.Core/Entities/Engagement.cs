namespace CultureLinkCRM.Core.Entities;

public class EngagementType
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<Engagement> Engagements { get; set; } = [];
}

public class Engagement : AuditableEntity
{
    public int? PersonId { get; set; }
    public Person? Person { get; set; }
    public int? OrganizationId { get; set; }
    public Organization? Organization { get; set; }

    public int EngagementTypeId { get; set; }
    public EngagementType? EngagementType { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Notes { get; set; } = string.Empty;
}
