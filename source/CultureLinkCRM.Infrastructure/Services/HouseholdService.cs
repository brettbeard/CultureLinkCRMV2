using CultureLinkCRM.Core.Dtos;
using CultureLinkCRM.Core.Entities;
using CultureLinkCRM.Core.Interfaces;
using CultureLinkCRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CultureLinkCRM.Infrastructure.Services;

public class HouseholdService(CultureLinkCrmDbContext db) : IHouseholdService
{
    public async Task<PagedResult<Household>> SearchAsync(string? nameFilter, int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var query = db.Households.Include(h => h.Members).AsQueryable();

        if (!string.IsNullOrWhiteSpace(nameFilter))
        {
            var term = nameFilter.Trim();
            query = query.Where(h => h.HouseholdName.Contains(term));
        }

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderBy(h => h.HouseholdName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<Household>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<Household?> GetByIdAsync(int id, CancellationToken ct = default) =>
        await db.Households
            .Include(h => h.Addresses)
            .Include(h => h.Phones)
            .Include(h => h.Emails)
            .Include(h => h.Members)
            .FirstOrDefaultAsync(h => h.Id == id, ct);

    public async Task<ServiceResult<Household>> CreateAsync(Household household, CancellationToken ct = default)
    {
        var validationError = Validate(household);
        if (validationError is not null)
        {
            return ServiceResult<Household>.Failure(validationError);
        }

        db.Households.Add(household);
        await db.SaveChangesAsync(ct);
        return ServiceResult<Household>.Success(household);
    }

    public async Task<ServiceResult<Household>> UpdateAsync(Household household, CancellationToken ct = default)
    {
        var validationError = Validate(household);
        if (validationError is not null)
        {
            return ServiceResult<Household>.Failure(validationError);
        }

        var existing = await db.Households
            .Include(h => h.Addresses)
            .Include(h => h.Phones)
            .Include(h => h.Emails)
            .FirstOrDefaultAsync(h => h.Id == household.Id, ct);

        if (existing is null)
        {
            return ServiceResult<Household>.Failure("Household not found.");
        }

        existing.HouseholdName = household.HouseholdName;
        existing.MailPreference = household.MailPreference;

        db.HouseholdAddresses.RemoveRange(existing.Addresses);
        existing.Addresses.Clear();
        foreach (var a in household.Addresses) existing.Addresses.Add(a);

        db.HouseholdPhones.RemoveRange(existing.Phones);
        existing.Phones.Clear();
        foreach (var p in household.Phones) existing.Phones.Add(p);

        db.HouseholdEmails.RemoveRange(existing.Emails);
        existing.Emails.Clear();
        foreach (var e in household.Emails) existing.Emails.Add(e);

        await db.SaveChangesAsync(ct);
        return ServiceResult<Household>.Success(existing);
    }

    public async Task<ServiceResult> DeleteAsync(int id, CancellationToken ct = default)
    {
        var household = await db.Households.Include(h => h.Members).FirstOrDefaultAsync(h => h.Id == id, ct);
        if (household is null)
        {
            return ServiceResult.Failure("Household not found.");
        }

        if (household.Members.Count > 0)
        {
            return ServiceResult.Failure(
                $"This Household cannot be deleted while it has {household.Members.Count} member(s) attached. Reassign or unlink all members first.");
        }

        db.Households.Remove(household);
        await db.SaveChangesAsync(ct);
        return ServiceResult.Success();
    }

    private static string? Validate(Household household)
    {
        if (string.IsNullOrWhiteSpace(household.HouseholdName))
        {
            return "Household name is required.";
        }

        var emailError = ContactValidation.ValidateEmails(household.Emails.Select(e => e.Address));
        if (emailError is not null)
        {
            return emailError;
        }

        var phoneError = ContactValidation.ValidatePhones(household.Phones.Select(p => p.Number));
        if (phoneError is not null)
        {
            return phoneError;
        }

        return ContactValidation.ValidatePrimaryFlags(household.Addresses.Select(a => (a.Type, a.IsPrimary)), "address")
            ?? ContactValidation.ValidatePrimaryFlags(household.Phones.Select(p => (p.Type, p.IsPrimary)), "phone")
            ?? ContactValidation.ValidatePrimaryFlags(household.Emails.Select(e => (e.Type, e.IsPrimary)), "email");
    }
}
