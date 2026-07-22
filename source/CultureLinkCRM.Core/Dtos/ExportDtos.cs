namespace CultureLinkCRM.Core.Dtos;

/// <summary>A single named/valued cell available for export, keyed by a stable column key.</summary>
public class ExportRow
{
    public Dictionary<string, string> Values { get; init; } = [];
}

public class ExportColumnDefinition
{
    public required string Key { get; init; }
    public required string DisplayName { get; init; }
}
