using CultureLinkCRM.Core.Dtos;
using CultureLinkCRM.Core.Entities;

namespace CultureLinkCRM.Core.Interfaces;

public interface INetworkService
{
    Task<IReadOnlyList<Network>> GetAllAsync(CancellationToken ct = default);
    Task<Network?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ServiceResult<Network>> CreateAsync(Network network, CancellationToken ct = default);
    Task<ServiceResult<Network>> UpdateAsync(Network network, CancellationToken ct = default);
    Task<ServiceResult> DeleteAsync(int id, CancellationToken ct = default);
}
