using CultureLinkCRM.Core.Dtos;
using CultureLinkCRM.Core.Entities;

namespace CultureLinkCRM.Core.Interfaces;

public interface IPersonService
{
    Task<PagedResult<Person>> SearchAsync(PersonFilter filter, CancellationToken ct = default);
    Task<Person?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ServiceResult<Person>> CreateAsync(Person person, CancellationToken ct = default);
    Task<ServiceResult<Person>> UpdateAsync(Person person, CancellationToken ct = default);
    Task<ServiceResult> DeleteAsync(int id, CancellationToken ct = default);
    Task<EffectiveContactInfo> GetEffectiveContactInfoAsync(int personId, CancellationToken ct = default);
}
