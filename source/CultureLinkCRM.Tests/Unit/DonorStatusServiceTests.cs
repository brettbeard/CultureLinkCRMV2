using CultureLinkCRM.Core.Enums;
using CultureLinkCRM.Infrastructure.Services;
using CultureLinkCRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CultureLinkCRM.Tests.Unit;

/// <summary>Direct unit tests for the lapsed-donor threshold calculation (Ref: FR-6), including the exact boundary condition.</summary>
public class DonorStatusServiceTests
{
    private static DonorStatusService CreateService()
    {
        var options = new DbContextOptionsBuilder<CultureLinkCrmDbContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;
        return new DonorStatusService(new CultureLinkCrmDbContext(options));
    }

    [Fact]
    public void NoDonationHistory_ReturnsNoDonationHistory()
    {
        var service = CreateService();
        var result = service.GetDonorStatus(null, 12, DateTime.UtcNow);
        Assert.Equal(DonorStatus.NoDonationHistory, result);
    }

    [Fact]
    public void DonationWithinThreshold_ReturnsActive()
    {
        var service = CreateService();
        var asOf = new DateTime(2026, 7, 21, 0, 0, 0, DateTimeKind.Utc);
        var lastDonation = asOf.AddMonths(-6);
        Assert.Equal(DonorStatus.Active, service.GetDonorStatus(lastDonation, 12, asOf));
    }

    [Fact]
    public void DonationExactlyAtThreshold_IsStillActive()
    {
        var service = CreateService();
        var asOf = new DateTime(2026, 7, 21, 0, 0, 0, DateTimeKind.Utc);
        var lastDonation = asOf.AddMonths(-12);
        Assert.Equal(DonorStatus.Active, service.GetDonorStatus(lastDonation, 12, asOf));
    }

    [Fact]
    public void DonationOneDayPastThreshold_IsLapsed()
    {
        var service = CreateService();
        var asOf = new DateTime(2026, 7, 21, 0, 0, 0, DateTimeKind.Utc);
        var lastDonation = asOf.AddMonths(-12).AddDays(-1);
        Assert.Equal(DonorStatus.Lapsed, service.GetDonorStatus(lastDonation, 12, asOf));
    }
}
