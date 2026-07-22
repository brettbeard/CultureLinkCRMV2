using CultureLinkCRM.Core.Dtos;
using CultureLinkCRM.Core.Entities;
using CultureLinkCRM.Core.Interfaces;
using CultureLinkCRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CultureLinkCRM.Infrastructure.Services;

public class PersonService(CultureLinkCrmDbContext db) : IPersonService
{
    public async Task<PagedResult<Person>> SearchAsync(PersonFilter filter, CancellationToken ct = default)
    {
        var query = db.People
            .Include(p => p.Household)
            .Include(p => p.Addresses)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Name))
        {
            var term = filter.Name.Trim();
            query = query.Where(p => p.FirstName.Contains(term) || p.LastName.Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(filter.City))
        {
            var city = filter.City.Trim();
            query = query.Where(p => p.Addresses.Any(a => a.City.Contains(city))
                || (p.Household != null && p.Household.Addresses.Any(a => a.City.Contains(city))));
        }

        if (filter.SegmentId is int segmentId)
        {
            query = query.Where(p => p.SegmentAssignments.Any(a => a.SegmentId == segmentId));
        }

        if (filter.NetworkId is int networkId)
        {
            query = query.Where(p => p.NetworkLinks.Any(l => l.NetworkId == networkId));
        }

        if (filter.AddedFrom is DateTime addedFrom)
        {
            query = query.Where(p => p.CreatedAt >= addedFrom);
        }

        if (filter.AddedTo is DateTime addedTo)
        {
            query = query.Where(p => p.CreatedAt <= addedTo);
        }

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderBy(p => p.LastName).ThenBy(p => p.FirstName)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(ct);

        return new PagedResult<Person>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize
        };
    }

    public async Task<Person?> GetByIdAsync(int id, CancellationToken ct = default) =>
        await db.People
            .Include(p => p.Household)
            .Include(p => p.Addresses)
            .Include(p => p.Phones)
            .Include(p => p.Emails)
            .Include(p => p.OrganizationLinks).ThenInclude(l => l.Organization)
            .Include(p => p.NetworkLinks).ThenInclude(l => l.Network)
            .Include(p => p.SegmentAssignments).ThenInclude(a => a.Segment)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<ServiceResult<Person>> CreateAsync(Person person, CancellationToken ct = default)
    {
        var validationError = Validate(person);
        if (validationError is not null)
        {
            return ServiceResult<Person>.Failure(validationError);
        }

        db.People.Add(person);
        await db.SaveChangesAsync(ct);
        return ServiceResult<Person>.Success(person);
    }

    public async Task<ServiceResult<Person>> UpdateAsync(Person person, CancellationToken ct = default)
    {
        var validationError = Validate(person);
        if (validationError is not null)
        {
            return ServiceResult<Person>.Failure(validationError);
        }

        var existing = await db.People
            .Include(p => p.Addresses)
            .Include(p => p.Phones)
            .Include(p => p.Emails)
            .FirstOrDefaultAsync(p => p.Id == person.Id, ct);

        if (existing is null)
        {
            return ServiceResult<Person>.Failure("Person not found.");
        }

        existing.FirstName = person.FirstName;
        existing.LastName = person.LastName;
        existing.MiddleName = person.MiddleName;
        existing.Suffix = person.Suffix;
        existing.HouseholdId = person.HouseholdId;

        ReplaceContactCollection(existing.Addresses, person.Addresses, db.PersonAddresses);
        ReplaceContactCollection(existing.Phones, person.Phones, db.PersonPhones);
        ReplaceContactCollection(existing.Emails, person.Emails, db.PersonEmails);

        await db.SaveChangesAsync(ct);
        return ServiceResult<Person>.Success(existing);
    }

    public async Task<ServiceResult> DeleteAsync(int id, CancellationToken ct = default)
    {
        var person = await db.People
            .Include(p => p.SegmentAssignments)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        if (person is null)
        {
            return ServiceResult.Failure("Person not found.");
        }

        var hasHistory =
            await db.Donations.AnyAsync(d => d.PersonId == id, ct) ||
            await db.CurriculumOrders.AnyAsync(o => o.PersonId == id, ct) ||
            await db.Engagements.AnyAsync(e => e.PersonId == id, ct) ||
            await db.SeminarAttendances.AnyAsync(a => a.PersonId == id, ct) ||
            person.SegmentAssignments.Count > 0;

        if (hasHistory)
        {
            return ServiceResult.Failure("This Person has donations, engagements, seminar attendance, curriculum orders, or Segment assignments attached and cannot be deleted. Remove that history first.");
        }

        db.People.Remove(person);
        await db.SaveChangesAsync(ct);
        return ServiceResult.Success();
    }

    public async Task<EffectiveContactInfo> GetEffectiveContactInfoAsync(int personId, CancellationToken ct = default)
    {
        var person = await db.People
            .Include(p => p.Addresses)
            .Include(p => p.Phones)
            .Include(p => p.Emails)
            .Include(p => p.Household).ThenInclude(h => h!.Addresses)
            .Include(p => p.Household).ThenInclude(h => h!.Phones)
            .Include(p => p.Household).ThenInclude(h => h!.Emails)
            .FirstOrDefaultAsync(p => p.Id == personId, ct);

        if (person is null)
        {
            return new EffectiveContactInfo();
        }

        return new EffectiveContactInfo
        {
            Addresses = [.. person.Addresses],
            Phones = [.. person.Phones],
            Emails = [.. person.Emails],
            HouseholdAddresses = person.Addresses.Count == 0 ? [.. person.Household?.Addresses ?? []] : [],
            HouseholdPhones = person.Phones.Count == 0 ? [.. person.Household?.Phones ?? []] : [],
            HouseholdEmails = person.Emails.Count == 0 ? [.. person.Household?.Emails ?? []] : []
        };
    }

    private static string? Validate(Person person)
    {
        if (string.IsNullOrWhiteSpace(person.FirstName) || string.IsNullOrWhiteSpace(person.LastName))
        {
            return "First and last name are required.";
        }

        var emailError = ContactValidation.ValidateEmails(person.Emails.Select(e => e.Address));
        if (emailError is not null)
        {
            return emailError;
        }

        var phoneError = ContactValidation.ValidatePhones(person.Phones.Select(p => p.Number));
        if (phoneError is not null)
        {
            return phoneError;
        }

        return ContactValidation.ValidatePrimaryFlags(person.Addresses.Select(a => (a.Type, a.IsPrimary)), "address")
            ?? ContactValidation.ValidatePrimaryFlags(person.Phones.Select(p => (p.Type, p.IsPrimary)), "phone")
            ?? ContactValidation.ValidatePrimaryFlags(person.Emails.Select(e => (e.Type, e.IsPrimary)), "email");
    }

    private static void ReplaceContactCollection<T>(ICollection<T> existing, ICollection<T> incoming, DbSet<T> dbSet) where T : class
    {
        dbSet.RemoveRange(existing);
        existing.Clear();
        foreach (var item in incoming)
        {
            existing.Add(item);
        }
    }
}
