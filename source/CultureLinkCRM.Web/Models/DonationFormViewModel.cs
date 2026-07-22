using System.ComponentModel.DataAnnotations;

namespace CultureLinkCRM.Web.Models;

public class DonationFormViewModel
{
    public int? PersonId { get; set; }
    public int? OrganizationId { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
    public decimal Amount { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public DateTime DonationDate { get; set; } = DateTime.Today;

    [StringLength(300)]
    public string FundProjectDesignation { get; set; } = string.Empty;
}

public class CurriculumOrderFormViewModel
{
    public int? PersonId { get; set; }
    public int? OrganizationId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than zero.")]
    public int Quantity { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public DateTime OrderDate { get; set; } = DateTime.Today;

    public int? LinkedOrganizationId { get; set; }
}

public class EngagementFormViewModel
{
    public int? PersonId { get; set; }
    public int? OrganizationId { get; set; }

    [Required]
    public int EngagementTypeId { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public DateTime StartDate { get; set; } = DateTime.Today;

    [DataType(DataType.Date)]
    public DateTime? EndDate { get; set; }

    [StringLength(4000)]
    public string Notes { get; set; } = string.Empty;
}
