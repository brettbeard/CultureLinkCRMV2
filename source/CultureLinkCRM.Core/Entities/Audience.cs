namespace CultureLinkCRM.Core.Entities;

public class Audience : AuditableEntity
{
    public string Name { get; set; } = string.Empty;

    public ICollection<AudienceSegment> SegmentLinks { get; set; } = [];
}

public class AudienceSegment
{
    public int Id { get; set; }
    public int AudienceId { get; set; }
    public Audience? Audience { get; set; }
    public int SegmentId { get; set; }
    public Segment? Segment { get; set; }
}
