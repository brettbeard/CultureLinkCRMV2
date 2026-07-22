using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CultureLinkCRM.Infrastructure.Data;

/// <summary>Used only by `dotnet ef` design-time tooling (migrations); the running app configures the DbContext via DI (see ServiceCollectionExtensions).</summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<CultureLinkCrmDbContext>
{
    public CultureLinkCrmDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CultureLinkCrmDbContext>();
        optionsBuilder.UseSqlite("Data Source=culturelinkcrm.db");
        return new CultureLinkCrmDbContext(optionsBuilder.Options);
    }
}
