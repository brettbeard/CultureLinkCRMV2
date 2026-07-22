using CultureLinkCRM.Core.Dtos;
using CultureLinkCRM.Core.Entities;

namespace CultureLinkCRM.Core.Interfaces;

public interface ISeminarService
{
    Task<IReadOnlyList<Seminar>> GetAllAsync(CancellationToken ct = default);
    Task<Seminar?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ServiceResult<Seminar>> CreateAsync(Seminar seminar, CancellationToken ct = default);
    Task<ServiceResult<Seminar>> UpdateAsync(Seminar seminar, CancellationToken ct = default);
    Task<ServiceResult> DeleteAsync(int id, CancellationToken ct = default);
    Task<ServiceResult> RecordAttendanceAsync(int seminarId, int personId, CancellationToken ct = default);
    Task<ServiceResult> RemoveAttendanceAsync(int attendanceId, CancellationToken ct = default);
    Task<IReadOnlyList<SeminarAttendance>> GetAttendanceForPersonAsync(int personId, CancellationToken ct = default);
}

public interface ICurriculumOrderService
{
    Task<IReadOnlyList<CurriculumOrder>> GetForPersonAsync(int personId, CancellationToken ct = default);
    Task<IReadOnlyList<CurriculumOrder>> GetForOrganizationAsync(int organizationId, CancellationToken ct = default);
    Task<ServiceResult<CurriculumOrder>> CreateAsync(CurriculumOrder order, CancellationToken ct = default);
    Task<ServiceResult> DeleteAsync(int id, CancellationToken ct = default);
}

public interface IEngagementService
{
    Task<IReadOnlyList<EngagementType>> GetTypesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Engagement>> GetForPersonAsync(int personId, CancellationToken ct = default);
    Task<IReadOnlyList<Engagement>> GetForOrganizationAsync(int organizationId, CancellationToken ct = default);
    Task<ServiceResult<Engagement>> CreateAsync(Engagement engagement, CancellationToken ct = default);
    Task<ServiceResult> DeleteAsync(int id, CancellationToken ct = default);
}
