using CultureLinkCRM.Core.Dtos;
using CultureLinkCRM.Core.Entities;
using CultureLinkCRM.Core.Interfaces;
using CultureLinkCRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CultureLinkCRM.Infrastructure.Services;

public class NetworkService(CultureLinkCrmDbContext db) : INetworkService
{
    public async Task<IReadOnlyList<Network>> GetAllAsync(CancellationToken ct = default) =>
        await db.Networks.Include(n => n.ParentNetwork).OrderBy(n => n.Name).ToListAsync(ct);

    public async Task<Network?> GetByIdAsync(int id, CancellationToken ct = default) =>
        await db.Networks
            .Include(n => n.ParentNetwork)
            .Include(n => n.ChildNetworks)
            .Include(n => n.PersonLinks).ThenInclude(l => l.Person)
            .Include(n => n.OrganizationLinks).ThenInclude(l => l.Organization)
            .FirstOrDefaultAsync(n => n.Id == id, ct);

    public async Task<ServiceResult<Network>> CreateAsync(Network network, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(network.Name))
        {
            return ServiceResult<Network>.Failure("Network name is required.");
        }

        db.Networks.Add(network);
        await db.SaveChangesAsync(ct);
        return ServiceResult<Network>.Success(network);
    }

    public async Task<ServiceResult<Network>> UpdateAsync(Network network, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(network.Name))
        {
            return ServiceResult<Network>.Failure("Network name is required.");
        }

        if (network.ParentNetworkId == network.Id)
        {
            return ServiceResult<Network>.Failure("A Network cannot be its own parent.");
        }

        var existing = await db.Networks.FirstOrDefaultAsync(n => n.Id == network.Id, ct);
        if (existing is null)
        {
            return ServiceResult<Network>.Failure("Network not found.");
        }

        existing.Name = network.Name;
        existing.NetworkType = network.NetworkType;
        existing.ParentNetworkId = network.ParentNetworkId;

        await db.SaveChangesAsync(ct);
        return ServiceResult<Network>.Success(existing);
    }

    public async Task<ServiceResult> DeleteAsync(int id, CancellationToken ct = default)
    {
        var network = await db.Networks.FirstOrDefaultAsync(n => n.Id == id, ct);
        if (network is null)
        {
            return ServiceResult.Failure("Network not found.");
        }

        var isReferenced =
            await db.Networks.AnyAsync(n => n.ParentNetworkId == id, ct) ||
            await db.PersonNetworks.AnyAsync(l => l.NetworkId == id, ct) ||
            await db.OrganizationNetworks.AnyAsync(l => l.NetworkId == id, ct);

        if (isReferenced)
        {
            return ServiceResult.Failure("This Network is referenced by a child Network or linked Person/Organization and cannot be deleted.");
        }

        db.Networks.Remove(network);
        await db.SaveChangesAsync(ct);
        return ServiceResult.Success();
    }
}
