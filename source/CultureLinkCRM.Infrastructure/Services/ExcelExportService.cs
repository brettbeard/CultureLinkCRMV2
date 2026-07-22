using ClosedXML.Excel;
using CultureLinkCRM.Core.Dtos;
using CultureLinkCRM.Core.Interfaces;

namespace CultureLinkCRM.Infrastructure.Services;

public class ExcelExportService : IExcelExportService
{
    public byte[] Generate(IReadOnlyList<ExportColumnDefinition> availableColumns, IReadOnlyList<string> selectedColumnKeys, IReadOnlyList<ExportRow> rows)
    {
        var columns = availableColumns.Where(c => selectedColumnKeys.Contains(c.Key)).ToList();

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Export");

        for (var col = 0; col < columns.Count; col++)
        {
            worksheet.Cell(1, col + 1).Value = columns[col].DisplayName;
            worksheet.Cell(1, col + 1).Style.Font.Bold = true;
        }

        for (var row = 0; row < rows.Count; row++)
        {
            for (var col = 0; col < columns.Count; col++)
            {
                rows[row].Values.TryGetValue(columns[col].Key, out var value);
                worksheet.Cell(row + 2, col + 1).Value = value ?? string.Empty;
            }
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
