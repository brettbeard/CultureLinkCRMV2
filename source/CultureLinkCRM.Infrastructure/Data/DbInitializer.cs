using CultureLinkCRM.Core.Entities;
using CultureLinkCRM.Core.Enums;
using CultureLinkCRM.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace CultureLinkCRM.Infrastructure.Data;

/// <summary>
/// Applies pending EF Core migrations and seeds the first Admin account (Ref: FR-12) from configuration
/// so the application is usable on first run with no external identity system to bootstrap from.
/// </summary>
public static class DbInitializer
{
    public static async Task InitializeAsync(CultureLinkCrmDbContext db, string? seedAdminEmail, string? seedAdminPassword, CancellationToken ct = default)
    {
        await db.Database.MigrateAsync(ct);

        if (await db.Users.AnyAsync(ct))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(seedAdminEmail) || string.IsNullOrWhiteSpace(seedAdminPassword))
        {
            return;
        }

        db.Users.Add(new User
        {
            Email = seedAdminEmail,
            PasswordHash = PasswordHasher.Hash(seedAdminPassword),
            Role = UserRole.Admin
        });

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            // Host startup can race with itself (e.g. under test hosting infrastructure that probes the
            // host builder before the real run); a unique-constraint failure here just means another
            // in-flight startup already seeded the Admin account, which is the desired end state.
        }
    }
}
