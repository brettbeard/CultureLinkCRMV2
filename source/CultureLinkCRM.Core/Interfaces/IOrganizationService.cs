using CultureLinkCRM.Core.Dtos;
using CultureLinkCRM.Core.Entities;

namespace CultureLinkCRM.Core.Interfaces;

public interface IOrganizationService
{
    Task<PagedResult<Organization>> SearchAsync(OrganizationFilter filter, CancellationToken ct = default);
    Task<Organization?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ServiceResult<Organization>> CreateAsync(Organization organization, CancellationToken ct = default);
    Task<ServiceResult<Organization>> UpdateAsync(Organization organization, CancellationToken ct = default);
    Task<ServiceResult> DeleteAsync(int id, CancellationToken ct = default);
}
