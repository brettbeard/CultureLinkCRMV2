using CultureLinkCRM.Core.Dtos;

namespace CultureLinkCRM.Core.Interfaces;

/// <summary>Resolves a filtered Person or Organization list to Household-deduplicated export rows (Ref: FR-11, sharing the FR-10 de-dup rule).</summary>
public interface IContactExportService
{
    Task<IReadOnlyList<AudienceMemberRow>> GetDedupedPersonRowsAsync(PersonFilter filter, CancellationToken ct = default);
    Task<IReadOnlyList<AudienceMemberRow>> GetDedupedOrganizationRowsAsync(OrganizationFilter filter, CancellationToken ct = default);
}
