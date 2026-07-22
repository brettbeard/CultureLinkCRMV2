using CultureLinkCRM.Core.Dtos;
using CultureLinkCRM.Core.Entities;
using CultureLinkCRM.Core.Enums;

namespace CultureLinkCRM.Core.Interfaces;

public interface IDonorStatusService
{
    /// <summary>Pure computation (Ref: FR-6): classifies donor status from the most recent donation date and the configured threshold.</summary>
    DonorStatus GetDonorStatus(DateTime? mostRecentDonationDate, int thresholdMonths, DateTime asOfDate);

    Task<int> GetLapsedThresholdMonthsAsync(CancellationToken ct = default);
    Task SetLapsedThresholdMonthsAsync(int months, CancellationToken ct = default);

    Task<DonorStatus> GetDonorStatusForPersonAsync(int personId, CancellationToken ct = default);
    Task<DonorStatus> GetDonorStatusForOrganizationAsync(int organizationId, CancellationToken ct = default);

    /// <summary>IDs of Persons/Organizations currently matching the given computed segment (Donor-Active or Donor-Lapsed).</summary>
    Task<(IReadOnlyList<int> PersonIds, IReadOnlyList<int> OrganizationIds)> GetComputedSegmentMembersAsync(int computedSegmentId, CancellationToken ct = default);
}

public interface IDonationService
{
    Task<IReadOnlyList<Donation>> GetForPersonAsync(int personId, CancellationToken ct = default);
    Task<IReadOnlyList<Donation>> GetForOrganizationAsync(int organizationId, CancellationToken ct = default);
    Task<ServiceResult<Donation>> CreateAsync(Donation donation, CancellationToken ct = default);
    Task<ServiceResult> DeleteAsync(int id, CancellationToken ct = default);
}
