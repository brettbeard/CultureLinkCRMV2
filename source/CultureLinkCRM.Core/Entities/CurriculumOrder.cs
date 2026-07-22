namespace CultureLinkCRM.Core.Entities;

public class CurriculumOrder : AuditableEntity
{
    public int? PersonId { get; set; }
    public Person? Person { get; set; }
    public int? OrganizationId { get; set; }
    public Organization? Organization { get; set; }

    public int Quantity { get; set; }
    public DateTime OrderDate { get; set; }

    public int? LinkedOrganizationId { get; set; }
    public Organization? LinkedOrganization { get; set; }
}
