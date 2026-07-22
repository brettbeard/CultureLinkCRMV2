using System.ComponentModel.DataAnnotations;

namespace CultureLinkCRM.Web.Models;

public class SettingsViewModel
{
    [Range(1, 120, ErrorMessage = "Threshold must be between 1 and 120 months.")]
    public int LapsedDonorThresholdMonths { get; set; }
}
