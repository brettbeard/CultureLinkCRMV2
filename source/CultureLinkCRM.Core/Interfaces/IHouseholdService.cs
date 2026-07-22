using CultureLinkCRM.Core.Dtos;
using CultureLinkCRM.Core.Entities;

namespace CultureLinkCRM.Core.Interfaces;

public interface IHouseholdService
{
    Task<PagedResult<Household>> SearchAsync(string? nameFilter, int pageNumber, int pageSize, CancellationToken ct = default);
    Task<Household?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ServiceResult<Household>> CreateAsync(Household household, CancellationToken ct = default);
    Task<ServiceResult<Household>> UpdateAsync(Household household, CancellationToken ct = default);
    Task<ServiceResult> DeleteAsync(int id, CancellationToken ct = default);
}
