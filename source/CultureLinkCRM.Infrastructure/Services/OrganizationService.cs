using CultureLinkCRM.Core.Dtos;
using CultureLinkCRM.Core.Entities;
using CultureLinkCRM.Core.Interfaces;
using CultureLinkCRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CultureLinkCRM.Infrastructure.Services;

public class OrganizationService(CultureLinkCrmDbContext db) : IOrganizationService
{
    public async Task<PagedResult<Organization>> SearchAsync(OrganizationFilter filter, CancellationToken ct = default)
    {
        var query = db.Organizations.Include(o => o.Addresses).AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Name))
        {
            var term = filter.Name.Trim();
            query = query.Where(o => o.Name.Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(filter.City))
        {
            var city = filter.City.Trim();
            query = query.Where(o => o.Addresses.Any(a => a.City.Contains(city)));
        }

        if (filter.SegmentId is int segmentId)
        {
            query = query.Where(o => o.SegmentAssignments.Any(a => a.SegmentId == segmentId));
        }

        if (filter.NetworkId is int networkId)
        {
            query = query.Where(o => o.NetworkLinks.Any(l => l.NetworkId == networkId));
        }

        if (filter.AddedFrom is DateTime addedFrom)
        {
            query = query.Where(o => o.CreatedAt >= addedFrom);
        }

        if (filter.AddedTo is DateTime addedTo)
        {
            query = query.Where(o => o.CreatedAt <= addedTo);
        }

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderBy(o => o.Name)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(ct);

        return new PagedResult<Organization>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize
        };
    }

    public async Task<Organization?> GetByIdAsync(int id, CancellationToken ct = default) =>
        await db.Organizations
            .Include(o => o.Addresses)
            .Include(o => o.Phones)
            .Include(o => o.Emails)
            .Include(o => o.PersonLinks).ThenInclude(l => l.Person)
            .Include(o => o.NetworkLinks).ThenInclude(l => l.Network)
            .Include(o => o.SegmentAssignments).ThenInclude(a => a.Segment)
            .FirstOrDefaultAsync(o => o.Id == id, ct);

    public async Task<ServiceResult<Organization>> CreateAsync(Organization organization, CancellationToken ct = default)
    {
        var validationError = Validate(organization);
        if (validationError is not null)
        {
            return ServiceResult<Organization>.Failure(validationError);
        }

        db.Organizations.Add(organization);
        await db.SaveChangesAsync(ct);
        return ServiceResult<Organization>.Success(organization);
    }

    public async Task<ServiceResult<Organization>> UpdateAsync(Organization organization, CancellationToken ct = default)
    {
        var validationError = Validate(organization);
        if (validationError is not null)
        {
            return ServiceResult<Organization>.Failure(validationError);
        }

        var existing = await db.Organizations
            .Include(o => o.Addresses)
            .Include(o => o.Phones)
            .Include(o => o.Emails)
            .FirstOrDefaultAsync(o => o.Id == organization.Id, ct);

        if (existing is null)
        {
            return ServiceResult<Organization>.Failure("Organization not found.");
        }

        existing.Name = organization.Name;
        existing.OrganizationType = organization.OrganizationType;

        db.OrganizationAddresses.RemoveRange(existing.Addresses);
        existing.Addresses.Clear();
        foreach (var a in organization.Addresses) existing.Addresses.Add(a);

        db.OrganizationPhones.RemoveRange(existing.Phones);
        existing.Phones.Clear();
        foreach (var p in organization.Phones) existing.Phones.Add(p);

        db.OrganizationEmails.RemoveRange(existing.Emails);
        existing.Emails.Clear();
        foreach (var e in organization.Emails) existing.Emails.Add(e);

        await db.SaveChangesAsync(ct);
        return ServiceResult<Organization>.Success(existing);
    }

    public async Task<ServiceResult> DeleteAsync(int id, CancellationToken ct = default)
    {
        var organization = await db.Organizations
            .Include(o => o.SegmentAssignments)
            .FirstOrDefaultAsync(o => o.Id == id, ct);

        if (organization is null)
        {
            return ServiceResult.Failure("Organization not found.");
        }

        var hasHistory =
            await db.Donations.AnyAsync(d => d.OrganizationId == id, ct) ||
            await db.CurriculumOrders.AnyAsync(o => o.OrganizationId == id || o.LinkedOrganizationId == id, ct) ||
            await db.Engagements.AnyAsync(e => e.OrganizationId == id, ct) ||
            organization.SegmentAssignments.Count > 0;

        if (hasHistory)
        {
            return ServiceResult.Failure("This Organization has donations, engagements, curriculum orders, or Segment assignments attached and cannot be deleted. Remove that history first.");
        }

        db.Organizations.Remove(organization);
        await db.SaveChangesAsync(ct);
        return ServiceResult.Success();
    }

    private static string? Validate(Organization organization)
    {
        if (string.IsNullOrWhiteSpace(organization.Name))
        {
            return "Organization name is required.";
        }

        var emailError = ContactValidation.ValidateEmails(organization.Emails.Select(e => e.Address));
        if (emailError is not null)
        {
            return emailError;
        }

        var phoneError = ContactValidation.ValidatePhones(organization.Phones.Select(p => p.Number));
        if (phoneError is not null)
        {
            return phoneError;
        }

        return ContactValidation.ValidatePrimaryFlags(organization.Addresses.Select(a => (a.Type, a.IsPrimary)), "address")
            ?? ContactValidation.ValidatePrimaryFlags(organization.Phones.Select(p => (p.Type, p.IsPrimary)), "phone")
            ?? ContactValidation.ValidatePrimaryFlags(organization.Emails.Select(e => (e.Type, e.IsPrimary)), "email");
    }
}
