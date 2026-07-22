namespace CultureLinkCRM.Core.Entities;

public class Donation : AuditableEntity
{
    public int? PersonId { get; set; }
    public Person? Person { get; set; }
    public int? OrganizationId { get; set; }
    public Organization? Organization { get; set; }

    public decimal Amount { get; set; }
    public DateTime DonationDate { get; set; }
    public string FundProjectDesignation { get; set; } = string.Empty;
}
