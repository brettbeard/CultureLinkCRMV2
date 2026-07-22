using CultureLinkCRM.Core.Dtos;
using CultureLinkCRM.Core.Entities;
using CultureLinkCRM.Core.Interfaces;
using CultureLinkCRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CultureLinkCRM.Infrastructure.Services;

/// <summary>
/// Resolves an Audience's live membership and applies the Household-level de-duplication rule shared by the
/// on-screen Audience view and the Excel export path (Ref: FR-10, FR-11).
/// </summary>
public class AudienceService(CultureLinkCrmDbContext db, IDonorStatusService donorStatusService) : IAudienceService
{
    public async Task<IReadOnlyList<Audience>> GetAllAsync(CancellationToken ct = default) =>
        await db.Audiences.Include(a => a.SegmentLinks).ThenInclude(l => l.Segment).OrderBy(a => a.Name).ToListAsync(ct);

    public async Task<Audience?> GetByIdAsync(int id, CancellationToken ct = default) =>
        await db.Audiences.Include(a => a.SegmentLinks).ThenInclude(l => l.Segment).FirstOrDefaultAsync(a => a.Id == id, ct);

    public async Task<ServiceResult<Audience>> CreateAsync(string name, IReadOnlyList<int> segmentIds, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return ServiceResult<Audience>.Failure("Audience name is required.");
        }

        if (segmentIds.Count == 0)
        {
            return ServiceResult<Audience>.Failure("An Audience must combine at least one Segment.");
        }

        var audience = new Audience { Name = name };
        foreach (var segmentId in segmentIds.Distinct())
        {
            audience.SegmentLinks.Add(new AudienceSegment { SegmentId = segmentId });
        }

        db.Audiences.Add(audience);
        await db.SaveChangesAsync(ct);
        return ServiceResult<Audience>.Success(audience);
    }

    public async Task<ServiceResult> DeleteAsync(int id, CancellationToken ct = default)
    {
        var audience = await db.Audiences.FirstOrDefaultAsync(a => a.Id == id, ct);
        if (audience is null)
        {
            return ServiceResult.Failure("Audience not found.");
        }

        db.Audiences.Remove(audience);
        await db.SaveChangesAsync(ct);
        return ServiceResult.Success();
    }

    public async Task<IReadOnlyList<AudienceMemberRow>> GetMembersAsync(int audienceId, CancellationToken ct = default)
    {
        var audience = await db.Audiences.Include(a => a.SegmentLinks).ThenInclude(l => l.Segment).FirstOrDefaultAsync(a => a.Id == audienceId, ct);
        if (audience is null)
        {
            return [];
        }

        var personIds = new HashSet<int>();
        var organizationIds = new HashSet<int>();

        foreach (var link in audience.SegmentLinks)
        {
            if (link.Segment is null)
            {
                continue;
            }

            if (link.Segment.IsComputed)
            {
                var (computedPersonIds, computedOrganizationIds) = await donorStatusService.GetComputedSegmentMembersAsync(link.Segment.Id, ct);
                foreach (var id in computedPersonIds) personIds.Add(id);
                foreach (var id in computedOrganizationIds) organizationIds.Add(id);
            }
            else
            {
                var assignments = await db.SegmentAssignments.Where(a => a.SegmentId == link.SegmentId).ToListAsync(ct);
                foreach (var assignment in assignments)
                {
                    if (assignment.PersonId is int pId) personIds.Add(pId);
                    if (assignment.OrganizationId is int oId) organizationIds.Add(oId);
                }
            }
        }

        var people = await db.People
            .Include(p => p.Addresses)
            .Include(p => p.Phones)
            .Include(p => p.Emails)
            .Include(p => p.Household).ThenInclude(h => h!.Addresses)
            .Include(p => p.Household).ThenInclude(h => h!.Phones)
            .Include(p => p.Household).ThenInclude(h => h!.Emails)
            .Where(p => personIds.Contains(p.Id))
            .ToListAsync(ct);

        var organizations = await db.Organizations
            .Include(o => o.Addresses)
            .Include(o => o.Phones)
            .Include(o => o.Emails)
            .Where(o => organizationIds.Contains(o.Id))
            .ToListAsync(ct);

        return HouseholdDedupBuilder.BuildRows(people, organizations);
    }
}
