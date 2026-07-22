using CultureLinkCRM.Core.Dtos;
using CultureLinkCRM.Core.Entities;

namespace CultureLinkCRM.Core.Interfaces;

public interface IAudienceService
{
    Task<IReadOnlyList<Audience>> GetAllAsync(CancellationToken ct = default);
    Task<Audience?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ServiceResult<Audience>> CreateAsync(string name, IReadOnlyList<int> segmentIds, CancellationToken ct = default);
    Task<ServiceResult> DeleteAsync(int id, CancellationToken ct = default);

    /// <summary>Live-evaluated, household-deduplicated membership for the given Audience (Ref: FR-10). Re-run on every call.</summary>
    Task<IReadOnlyList<AudienceMemberRow>> GetMembersAsync(int audienceId, CancellationToken ct = default);
}
