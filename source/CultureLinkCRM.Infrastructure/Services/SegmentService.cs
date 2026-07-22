using CultureLinkCRM.Core.Dtos;
using CultureLinkCRM.Core.Entities;
using CultureLinkCRM.Core.Interfaces;
using CultureLinkCRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CultureLinkCRM.Infrastructure.Services;

public class SegmentService(CultureLinkCrmDbContext db) : ISegmentService
{
    public async Task<IReadOnlyList<Segment>> GetAllAsync(bool includeComputed, CancellationToken ct = default) =>
        await db.Segments
            .Where(s => includeComputed || !s.IsComputed)
            .OrderBy(s => s.Name)
            .ToListAsync(ct);

    public async Task<Segment?> GetByIdAsync(int id, CancellationToken ct = default) =>
        await db.Segments
            .Include(s => s.Assignments).ThenInclude(a => a.Person)
            .Include(s => s.Assignments).ThenInclude(a => a.Organization)
            .FirstOrDefaultAsync(s => s.Id == id, ct);

    public async Task<ServiceResult<Segment>> CreateAsync(Segment segment, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(segment.Name))
        {
            return ServiceResult<Segment>.Failure("Segment name is required.");
        }

        segment.IsComputed = false;
        db.Segments.Add(segment);
        await db.SaveChangesAsync(ct);
        return ServiceResult<Segment>.Success(segment);
    }

    public async Task<ServiceResult<Segment>> UpdateAsync(Segment segment, CancellationToken ct = default)
    {
        var existing = await db.Segments.FirstOrDefaultAsync(s => s.Id == segment.Id, ct);
        if (existing is null)
        {
            return ServiceResult<Segment>.Failure("Segment not found.");
        }

        if (existing.IsComputed)
        {
            return ServiceResult<Segment>.Failure("Computed Segments (Donor - Active / Donor - Lapsed) cannot be edited.");
        }

        if (string.IsNullOrWhiteSpace(segment.Name))
        {
            return ServiceResult<Segment>.Failure("Segment name is required.");
        }

        existing.Name = segment.Name;
        await db.SaveChangesAsync(ct);
        return ServiceResult<Segment>.Success(existing);
    }

    public async Task<ServiceResult> DeleteAsync(int id, CancellationToken ct = default)
    {
        var segment = await db.Segments.Include(s => s.Assignments).FirstOrDefaultAsync(s => s.Id == id, ct);
        if (segment is null)
        {
            return ServiceResult.Failure("Segment not found.");
        }

        if (segment.IsComputed)
        {
            return ServiceResult.Failure("Computed Segments (Donor - Active / Donor - Lapsed) cannot be deleted.");
        }

        if (segment.Assignments.Count > 0 || await db.AudienceSegments.AnyAsync(l => l.SegmentId == id, ct))
        {
            return ServiceResult.Failure("This Segment is still assigned to contacts or used in an Audience and cannot be deleted.");
        }

        db.Segments.Remove(segment);
        await db.SaveChangesAsync(ct);
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> AssignAsync(int segmentId, int? personId, int? organizationId, CancellationToken ct = default)
    {
        if ((personId is null) == (organizationId is null))
        {
            return ServiceResult.Failure("A Segment assignment must reference exactly one Person or Organization.");
        }

        var segment = await db.Segments.FirstOrDefaultAsync(s => s.Id == segmentId, ct);
        if (segment is null)
        {
            return ServiceResult.Failure("Segment not found.");
        }

        if (segment.IsComputed)
        {
            return ServiceResult.Failure("Donor status Segments are computed automatically and cannot be manually assigned.");
        }

        db.SegmentAssignments.Add(new SegmentAssignment
        {
            SegmentId = segmentId,
            PersonId = personId,
            OrganizationId = organizationId,
            DateAssigned = DateTime.UtcNow
        });
        await db.SaveChangesAsync(ct);
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> UnassignAsync(int segmentAssignmentId, CancellationToken ct = default)
    {
        var assignment = await db.SegmentAssignments.FirstOrDefaultAsync(a => a.Id == segmentAssignmentId, ct);
        if (assignment is null)
        {
            return ServiceResult.Failure("Segment assignment not found.");
        }

        db.SegmentAssignments.Remove(assignment);
        await db.SaveChangesAsync(ct);
        return ServiceResult.Success();
    }
}
