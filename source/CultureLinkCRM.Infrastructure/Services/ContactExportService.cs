using CultureLinkCRM.Core.Dtos;
using CultureLinkCRM.Core.Interfaces;
using CultureLinkCRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CultureLinkCRM.Infrastructure.Services;

public class ContactExportService(CultureLinkCrmDbContext db, IPersonService personService, IOrganizationService organizationService) : IContactExportService
{
    private const int UnpagedSize = 100_000;

    public async Task<IReadOnlyList<AudienceMemberRow>> GetDedupedPersonRowsAsync(PersonFilter filter, CancellationToken ct = default)
    {
        var unpagedFilter = new PersonFilter
        {
            Name = filter.Name,
            City = filter.City,
            SegmentId = filter.SegmentId,
            NetworkId = filter.NetworkId,
            AddedFrom = filter.AddedFrom,
            AddedTo = filter.AddedTo,
            PageNumber = 1,
            PageSize = UnpagedSize
        };
        var matched = await personService.SearchAsync(unpagedFilter, ct);
        var ids = matched.Items.Select(p => p.Id).ToList();

        var people = await db.People
            .Include(p => p.Addresses)
            .Include(p => p.Phones)
            .Include(p => p.Emails)
            .Include(p => p.Household).ThenInclude(h => h!.Addresses)
            .Include(p => p.Household).ThenInclude(h => h!.Phones)
            .Include(p => p.Household).ThenInclude(h => h!.Emails)
            .Where(p => ids.Contains(p.Id))
            .ToListAsync(ct);

        return HouseholdDedupBuilder.BuildRows(people, []);
    }

    public async Task<IReadOnlyList<AudienceMemberRow>> GetDedupedOrganizationRowsAsync(OrganizationFilter filter, CancellationToken ct = default)
    {
        var unpagedFilter = new OrganizationFilter
        {
            Name = filter.Name,
            City = filter.City,
            SegmentId = filter.SegmentId,
            NetworkId = filter.NetworkId,
            AddedFrom = filter.AddedFrom,
            AddedTo = filter.AddedTo,
            PageNumber = 1,
            PageSize = UnpagedSize
        };
        var matched = await organizationService.SearchAsync(unpagedFilter, ct);
        var ids = matched.Items.Select(o => o.Id).ToList();

        var organizations = await db.Organizations
            .Include(o => o.Addresses)
            .Include(o => o.Phones)
            .Include(o => o.Emails)
            .Where(o => ids.Contains(o.Id))
            .ToListAsync(ct);

        return HouseholdDedupBuilder.BuildRows([], organizations);
    }
}
