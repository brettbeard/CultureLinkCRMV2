using CultureLinkCRM.Core.Dtos;
using CultureLinkCRM.Core.Entities;
using CultureLinkCRM.Core.Interfaces;
using CultureLinkCRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CultureLinkCRM.Infrastructure.Services;

public class SeminarService(CultureLinkCrmDbContext db) : ISeminarService
{
    public async Task<IReadOnlyList<Seminar>> GetAllAsync(CancellationToken ct = default) =>
        await db.Seminars.Include(s => s.ParentSeminar).OrderByDescending(s => s.Year).ThenBy(s => s.City).ToListAsync(ct);

    public async Task<Seminar?> GetByIdAsync(int id, CancellationToken ct = default) =>
        await db.Seminars
            .Include(s => s.ParentSeminar)
            .Include(s => s.ChildSeminars)
            .Include(s => s.Attendances).ThenInclude(a => a.Person)
            .FirstOrDefaultAsync(s => s.Id == id, ct);

    public async Task<ServiceResult<Seminar>> CreateAsync(Seminar seminar, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(seminar.City) || seminar.Year <= 0)
        {
            return ServiceResult<Seminar>.Failure("City and a valid Year are required.");
        }

        db.Seminars.Add(seminar);
        await db.SaveChangesAsync(ct);
        return ServiceResult<Seminar>.Success(seminar);
    }

    public async Task<ServiceResult<Seminar>> UpdateAsync(Seminar seminar, CancellationToken ct = default)
    {
        var existing = await db.Seminars.FirstOrDefaultAsync(s => s.Id == seminar.Id, ct);
        if (existing is null)
        {
            return ServiceResult<Seminar>.Failure("Seminar not found.");
        }

        if (seminar.ParentSeminarId == seminar.Id)
        {
            return ServiceResult<Seminar>.Failure("A Seminar cannot be its own parent.");
        }

        existing.City = seminar.City;
        existing.Year = seminar.Year;
        existing.ParentSeminarId = seminar.ParentSeminarId;
        await db.SaveChangesAsync(ct);
        return ServiceResult<Seminar>.Success(existing);
    }

    public async Task<ServiceResult> DeleteAsync(int id, CancellationToken ct = default)
    {
        var seminar = await db.Seminars.Include(s => s.Attendances).FirstOrDefaultAsync(s => s.Id == id, ct);
        if (seminar is null)
        {
            return ServiceResult.Failure("Seminar not found.");
        }

        var isReferenced = seminar.Attendances.Count > 0 || await db.Seminars.AnyAsync(s => s.ParentSeminarId == id, ct);
        if (isReferenced)
        {
            return ServiceResult.Failure("This Seminar has attendance records or child Seminars attached and cannot be deleted.");
        }

        db.Seminars.Remove(seminar);
        await db.SaveChangesAsync(ct);
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> RecordAttendanceAsync(int seminarId, int personId, CancellationToken ct = default)
    {
        var alreadyRecorded = await db.SeminarAttendances.AnyAsync(a => a.SeminarId == seminarId && a.PersonId == personId, ct);
        if (alreadyRecorded)
        {
            return ServiceResult.Failure("This Person is already recorded as attending this Seminar.");
        }

        db.SeminarAttendances.Add(new SeminarAttendance { SeminarId = seminarId, PersonId = personId });
        await db.SaveChangesAsync(ct);
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> RemoveAttendanceAsync(int attendanceId, CancellationToken ct = default)
    {
        var attendance = await db.SeminarAttendances.FirstOrDefaultAsync(a => a.Id == attendanceId, ct);
        if (attendance is null)
        {
            return ServiceResult.Failure("Attendance record not found.");
        }

        db.SeminarAttendances.Remove(attendance);
        await db.SaveChangesAsync(ct);
        return ServiceResult.Success();
    }

    public async Task<IReadOnlyList<SeminarAttendance>> GetAttendanceForPersonAsync(int personId, CancellationToken ct = default) =>
        await db.SeminarAttendances.Include(a => a.Seminar).Where(a => a.PersonId == personId).ToListAsync(ct);
}

public class CurriculumOrderService(CultureLinkCrmDbContext db) : ICurriculumOrderService
{
    public async Task<IReadOnlyList<CurriculumOrder>> GetForPersonAsync(int personId, CancellationToken ct = default) =>
        await db.CurriculumOrders.Include(o => o.LinkedOrganization).Where(o => o.PersonId == personId).OrderByDescending(o => o.OrderDate).ToListAsync(ct);

    public async Task<IReadOnlyList<CurriculumOrder>> GetForOrganizationAsync(int organizationId, CancellationToken ct = default) =>
        await db.CurriculumOrders.Where(o => o.OrganizationId == organizationId).OrderByDescending(o => o.OrderDate).ToListAsync(ct);

    public async Task<ServiceResult<CurriculumOrder>> CreateAsync(CurriculumOrder order, CancellationToken ct = default)
    {
        if ((order.PersonId is null) == (order.OrganizationId is null))
        {
            return ServiceResult<CurriculumOrder>.Failure("A curriculum order must be recorded against exactly one Person or Organization.");
        }

        if (order.Quantity <= 0)
        {
            return ServiceResult<CurriculumOrder>.Failure("Quantity must be greater than zero.");
        }

        db.CurriculumOrders.Add(order);
        await db.SaveChangesAsync(ct);
        return ServiceResult<CurriculumOrder>.Success(order);
    }

    public async Task<ServiceResult> DeleteAsync(int id, CancellationToken ct = default)
    {
        var order = await db.CurriculumOrders.FirstOrDefaultAsync(o => o.Id == id, ct);
        if (order is null)
        {
            return ServiceResult.Failure("Curriculum order not found.");
        }

        db.CurriculumOrders.Remove(order);
        await db.SaveChangesAsync(ct);
        return ServiceResult.Success();
    }
}

public class EngagementService(CultureLinkCrmDbContext db) : IEngagementService
{
    public async Task<IReadOnlyList<EngagementType>> GetTypesAsync(CancellationToken ct = default) =>
        await db.EngagementTypes.OrderBy(t => t.Name).ToListAsync(ct);

    public async Task<IReadOnlyList<Engagement>> GetForPersonAsync(int personId, CancellationToken ct = default) =>
        await db.Engagements.Include(e => e.EngagementType).Where(e => e.PersonId == personId).OrderByDescending(e => e.StartDate).ToListAsync(ct);

    public async Task<IReadOnlyList<Engagement>> GetForOrganizationAsync(int organizationId, CancellationToken ct = default) =>
        await db.Engagements.Include(e => e.EngagementType).Where(e => e.OrganizationId == organizationId).OrderByDescending(e => e.StartDate).ToListAsync(ct);

    public async Task<ServiceResult<Engagement>> CreateAsync(Engagement engagement, CancellationToken ct = default)
    {
        if ((engagement.PersonId is null) == (engagement.OrganizationId is null))
        {
            return ServiceResult<Engagement>.Failure("An engagement must be recorded against exactly one Person or Organization.");
        }

        if (engagement.EndDate is not null && engagement.EndDate < engagement.StartDate)
        {
            return ServiceResult<Engagement>.Failure("End date cannot be before start date.");
        }

        db.Engagements.Add(engagement);
        await db.SaveChangesAsync(ct);
        return ServiceResult<Engagement>.Success(engagement);
    }

    public async Task<ServiceResult> DeleteAsync(int id, CancellationToken ct = default)
    {
        var engagement = await db.Engagements.FirstOrDefaultAsync(e => e.Id == id, ct);
        if (engagement is null)
        {
            return ServiceResult.Failure("Engagement not found.");
        }

        db.Engagements.Remove(engagement);
        await db.SaveChangesAsync(ct);
        return ServiceResult.Success();
    }
}
