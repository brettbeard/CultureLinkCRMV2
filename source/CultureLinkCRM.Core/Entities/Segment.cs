namespace CultureLinkCRM.Core.Entities;

public class Segment : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public bool IsComputed { get; set; }

    public ICollection<SegmentAssignment> Assignments { get; set; } = [];
    public ICollection<AudienceSegment> AudienceLinks { get; set; } = [];
}

public class SegmentAssignment
{
    public int Id { get; set; }
    public int SegmentId { get; set; }
    public Segment? Segment { get; set; }
    public int? PersonId { get; set; }
    public Person? Person { get; set; }
    public int? OrganizationId { get; set; }
    public Organization? Organization { get; set; }
    public DateTime DateAssigned { get; set; }
}
