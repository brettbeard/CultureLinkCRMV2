using CultureLinkCRM.Core.Dtos;
using CultureLinkCRM.Core.Interfaces;
using CultureLinkCRM.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CultureLinkCRM.Web.Controllers;

// Available to all roles including read-only User (Ref: FR-13: Excel Export is explicitly granted to User).
[Authorize]
public class ExportController(IContactExportService contactExportService, IAudienceService audienceService, IExcelExportService excelExportService) : Controller
{
    private static readonly List<ExportColumnDefinition> Columns =
    [
        new() { Key = "Kind", DisplayName = "Row Type" },
        new() { Key = "Name", DisplayName = "Name" },
        new() { Key = "Street1", DisplayName = "Street" },
        new() { Key = "City", DisplayName = "City" },
        new() { Key = "StateProvince", DisplayName = "State/Province" },
        new() { Key = "PostalCode", DisplayName = "Postal Code" },
        new() { Key = "Country", DisplayName = "Country" },
        new() { Key = "Phone", DisplayName = "Phone" },
        new() { Key = "Email", DisplayName = "Email" }
    ];

    // Session-only column selection (Ref: FR-11): chosen at export time, never persisted as a per-user default.
    [HttpGet]
    public IActionResult Persons([FromQuery] PersonFilter filter) => View("ColumnPicker", new ExportColumnPickerViewModel
    {
        FormAction = nameof(PersonsDownload),
        AvailableColumns = Columns,
        SelectedColumnKeys = [.. Columns.Select(c => c.Key)],
        PersonFilter = filter
    });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PersonsDownload(PersonFilter filter, List<string> selectedColumnKeys, CancellationToken ct)
    {
        var rows = await contactExportService.GetDedupedPersonRowsAsync(filter, ct);
        return DownloadExcel(rows, selectedColumnKeys, "People.xlsx");
    }

    [HttpGet]
    public IActionResult Organizations([FromQuery] OrganizationFilter filter) => View("ColumnPicker", new ExportColumnPickerViewModel
    {
        FormAction = nameof(OrganizationsDownload),
        AvailableColumns = Columns,
        SelectedColumnKeys = [.. Columns.Select(c => c.Key)],
        OrganizationFilter = filter
    });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> OrganizationsDownload(OrganizationFilter filter, List<string> selectedColumnKeys, CancellationToken ct)
    {
        var rows = await contactExportService.GetDedupedOrganizationRowsAsync(filter, ct);
        return DownloadExcel(rows, selectedColumnKeys, "Organizations.xlsx");
    }

    [HttpGet]
    public IActionResult Audience(int id) => View("ColumnPicker", new ExportColumnPickerViewModel
    {
        FormAction = nameof(AudienceDownload),
        AvailableColumns = Columns,
        SelectedColumnKeys = [.. Columns.Select(c => c.Key)],
        AudienceId = id
    });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AudienceDownload(int audienceId, List<string> selectedColumnKeys, CancellationToken ct)
    {
        var rows = await audienceService.GetMembersAsync(audienceId, ct);
        return DownloadExcel(rows, selectedColumnKeys, "Audience.xlsx");
    }

    private FileContentResult DownloadExcel(IReadOnlyList<AudienceMemberRow> rows, List<string> selectedColumnKeys, string fileName)
    {
        var exportRows = rows.Select(r => new ExportRow
        {
            Values = new Dictionary<string, string>
            {
                ["Kind"] = r.Kind.ToString(),
                ["Name"] = r.DisplayName,
                ["Street1"] = r.Street1 ?? string.Empty,
                ["City"] = r.City ?? string.Empty,
                ["StateProvince"] = r.StateProvince ?? string.Empty,
                ["PostalCode"] = r.PostalCode ?? string.Empty,
                ["Country"] = r.Country ?? string.Empty,
                ["Phone"] = r.Phone ?? string.Empty,
                ["Email"] = r.Email ?? string.Empty
            }
        }).ToList();

        var bytes = excelExportService.Generate(Columns, selectedColumnKeys, exportRows);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }
}
