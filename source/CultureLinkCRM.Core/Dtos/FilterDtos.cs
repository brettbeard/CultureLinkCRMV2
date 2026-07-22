namespace CultureLinkCRM.Core.Dtos;

public class PersonFilter
{
    public string? Name { get; set; }
    public string? City { get; set; }
    public int? SegmentId { get; set; }
    public int? NetworkId { get; set; }
    public DateTime? AddedFrom { get; set; }
    public DateTime? AddedTo { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

public class OrganizationFilter
{
    public string? Name { get; set; }
    public string? City { get; set; }
    public int? SegmentId { get; set; }
    public int? NetworkId { get; set; }
    public DateTime? AddedFrom { get; set; }
    public DateTime? AddedTo { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}
