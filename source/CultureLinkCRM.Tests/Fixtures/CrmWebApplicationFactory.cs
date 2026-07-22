using CultureLinkCRM.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CultureLinkCRM.Tests.Fixtures;

/// <summary>
/// Api-boundary test host (Ref: testing approach in the design spec / prior spec's Testing Decisions):
/// drives the application through its real Controllers against a real EF Core DbContext backed by a
/// fresh, uniquely-named SQLite file database per test class, with a known seeded Admin account.
/// </summary>
public class CrmWebApplicationFactory : WebApplicationFactory<Program>
{
    public readonly string DbPath = Path.Combine(Path.GetTempPath(), $"culturelinkcrm-tests-{Guid.NewGuid():N}.db");
    public const string SeedAdminEmail = "admin@test.local";
    public const string SeedAdminPassword = "TestAdmin123!";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = $"Data Source={DbPath}",
                ["Seed:AdminEmail"] = SeedAdminEmail,
                ["Seed:AdminPassword"] = SeedAdminPassword
            });
        });
    }

    public async Task<CultureLinkCrmDbContext> GetDbContextAsync()
    {
        var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CultureLinkCrmDbContext>();
        await Task.CompletedTask;
        return db;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (File.Exists(DbPath))
        {
            File.Delete(DbPath);
        }
    }
}
