using CultureLinkCRM.Core.Dtos;

namespace CultureLinkCRM.Web.Models;

public class ExportColumnPickerViewModel
{
    public required string FormAction { get; init; }
    public required List<ExportColumnDefinition> AvailableColumns { get; init; }
    public List<string> SelectedColumnKeys { get; set; } = [];
    public PersonFilter? PersonFilter { get; init; }
    public OrganizationFilter? OrganizationFilter { get; init; }
    public int? AudienceId { get; init; }
}
