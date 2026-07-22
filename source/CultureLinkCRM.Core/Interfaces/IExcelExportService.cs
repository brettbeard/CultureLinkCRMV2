using CultureLinkCRM.Core.Dtos;

namespace CultureLinkCRM.Core.Interfaces;

public interface IExcelExportService
{
    /// <summary>Builds an .xlsx file in-memory from the given rows, restricted to the selected column keys, in the given order.</summary>
    byte[] Generate(IReadOnlyList<ExportColumnDefinition> availableColumns, IReadOnlyList<string> selectedColumnKeys, IReadOnlyList<ExportRow> rows);
}
