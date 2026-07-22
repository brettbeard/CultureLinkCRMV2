using CultureLinkCRM.Core;
using CultureLinkCRM.Core.Dtos;
using CultureLinkCRM.Core.Entities;
using CultureLinkCRM.Core.Enums;
using CultureLinkCRM.Core.Interfaces;
using CultureLinkCRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CultureLinkCRM.Infrastructure.Services;

public class DonorStatusService(CultureLinkCrmDbContext db) : IDonorStatusService
{
    public DonorStatus GetDonorStatus(DateTime? mostRecentDonationDate, int thresholdMonths, DateTime asOfDate)
    {
        if (mostRecentDonationDate is null)
        {
            return DonorStatus.NoDonationHistory;
        }

        var thresholdDate = asOfDate.AddMonths(-thresholdMonths);
        return mostRecentDonationDate.Value >= thresholdDate ? DonorStatus.Active : DonorStatus.Lapsed;
    }

    public async Task<int> GetLapsedThresholdMonthsAsync(CancellationToken ct = default)
    {
        var setting = await db.SystemSettings.FirstOrDefaultAsync(s => s.Key == SystemSetting.LapsedDonorThresholdMonthsKey, ct);
        if (setting is null || !int.TryParse(setting.Value, out var months))
        {
            return int.Parse(SystemSetting.DefaultLapsedDonorThresholdMonths);
        }
        return months;
    }

    public async Task SetLapsedThresholdMonthsAsync(int months, CancellationToken ct = default)
    {
        if (months <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(months), "Lapsed donor threshold must be a positive number of months.");
        }

        var setting = await db.SystemSettings.FirstOrDefaultAsync(s => s.Key == SystemSetting.LapsedDonorThresholdMonthsKey, ct);
        if (setting is null)
        {
            db.SystemSettings.Add(new SystemSetting { Key = SystemSetting.LapsedDonorThresholdMonthsKey, Value = months.ToString() });
        }
        else
        {
            setting.Value = months.ToString();
        }
        await db.SaveChangesAsync(ct);
    }

    public async Task<DonorStatus> GetDonorStatusForPersonAsync(int personId, CancellationToken ct = default)
    {
        var mostRecent = await db.Donations
            .Where(d => d.PersonId == personId)
            .OrderByDescending(d => d.DonationDate)
            .Select(d => (DateTime?)d.DonationDate)
            .FirstOrDefaultAsync(ct);

        var threshold = await GetLapsedThresholdMonthsAsync(ct);
        return GetDonorStatus(mostRecent, threshold, DateTime.UtcNow);
    }

    public async Task<DonorStatus> GetDonorStatusForOrganizationAsync(int organizationId, CancellationToken ct = default)
    {
        var mostRecent = await db.Donations
            .Where(d => d.OrganizationId == organizationId)
            .OrderByDescending(d => d.DonationDate)
            .Select(d => (DateTime?)d.DonationDate)
            .FirstOrDefaultAsync(ct);

        var threshold = await GetLapsedThresholdMonthsAsync(ct);
        return GetDonorStatus(mostRecent, threshold, DateTime.UtcNow);
    }

    public async Task<(IReadOnlyList<int> PersonIds, IReadOnlyList<int> OrganizationIds)> GetComputedSegmentMembersAsync(int computedSegmentId, CancellationToken ct = default)
    {
        var wantActive = computedSegmentId == SeedIds.DonorActiveSegmentId;
        var threshold = await GetLapsedThresholdMonthsAsync(ct);
        var asOf = DateTime.UtcNow;

        var personLastDonation = await db.Donations
            .Where(d => d.PersonId != null)
            .GroupBy(d => d.PersonId!.Value)
            .Select(g => new { PersonId = g.Key, LastDate = g.Max(d => d.DonationDate) })
            .ToListAsync(ct);

        var orgLastDonation = await db.Donations
            .Where(d => d.OrganizationId != null)
            .GroupBy(d => d.OrganizationId!.Value)
            .Select(g => new { OrganizationId = g.Key, LastDate = g.Max(d => d.DonationDate) })
            .ToListAsync(ct);

        var personIds = personLastDonation
            .Where(p => (GetDonorStatus(p.LastDate, threshold, asOf) == DonorStatus.Active) == wantActive)
            .Select(p => p.PersonId)
            .ToList();

        var organizationIds = orgLastDonation
            .Where(o => (GetDonorStatus(o.LastDate, threshold, asOf) == DonorStatus.Active) == wantActive)
            .Select(o => o.OrganizationId)
            .ToList();

        return (personIds, organizationIds);
    }
}

public class DonationService(CultureLinkCrmDbContext db) : IDonationService
{
    public async Task<IReadOnlyList<Donation>> GetForPersonAsync(int personId, CancellationToken ct = default) =>
        await db.Donations.Where(d => d.PersonId == personId).OrderByDescending(d => d.DonationDate).ToListAsync(ct);

    public async Task<IReadOnlyList<Donation>> GetForOrganizationAsync(int organizationId, CancellationToken ct = default) =>
        await db.Donations.Where(d => d.OrganizationId == organizationId).OrderByDescending(d => d.DonationDate).ToListAsync(ct);

    public async Task<ServiceResult<Donation>> CreateAsync(Donation donation, CancellationToken ct = default)
    {
        if ((donation.PersonId is null) == (donation.OrganizationId is null))
        {
            return ServiceResult<Donation>.Failure("A donation must be recorded against exactly one Person or Organization.");
        }

        if (donation.Amount <= 0)
        {
            return ServiceResult<Donation>.Failure("Donation amount must be greater than zero.");
        }

        db.Donations.Add(donation);
        await db.SaveChangesAsync(ct);
        return ServiceResult<Donation>.Success(donation);
    }

    public async Task<ServiceResult> DeleteAsync(int id, CancellationToken ct = default)
    {
        var donation = await db.Donations.FirstOrDefaultAsync(d => d.Id == id, ct);
        if (donation is null)
        {
            return ServiceResult.Failure("Donation not found.");
        }

        db.Donations.Remove(donation);
        await db.SaveChangesAsync(ct);
        return ServiceResult.Success();
    }
}
