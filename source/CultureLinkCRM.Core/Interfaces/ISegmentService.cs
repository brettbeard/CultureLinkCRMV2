using CultureLinkCRM.Core.Dtos;
using CultureLinkCRM.Core.Entities;

namespace CultureLinkCRM.Core.Interfaces;

public interface ISegmentService
{
    /// <summary>All Segments, optionally including the system-managed computed (IsComputed) rows.</summary>
    Task<IReadOnlyList<Segment>> GetAllAsync(bool includeComputed, CancellationToken ct = default);
    Task<Segment?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ServiceResult<Segment>> CreateAsync(Segment segment, CancellationToken ct = default);
    Task<ServiceResult<Segment>> UpdateAsync(Segment segment, CancellationToken ct = default);
    Task<ServiceResult> DeleteAsync(int id, CancellationToken ct = default);
    Task<ServiceResult> AssignAsync(int segmentId, int? personId, int? organizationId, CancellationToken ct = default);
    Task<ServiceResult> UnassignAsync(int segmentAssignmentId, CancellationToken ct = default);
}
