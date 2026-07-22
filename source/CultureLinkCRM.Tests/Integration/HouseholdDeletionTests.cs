using CultureLinkCRM.Core.Entities;
using CultureLinkCRM.Infrastructure.Data;
using CultureLinkCRM.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CultureLinkCRM.Tests.Integration;

/// <summary>Ref: FR-2 acceptance criteria — deleting a Household with members attached is blocked with a clear error, and nothing is deleted.</summary>
public class HouseholdDeletionTests(CrmWebApplicationFactory factory) : IClassFixture<CrmWebApplicationFactory>
{
    [Fact]
    public async Task DeletingHouseholdWithMembers_IsBlocked()
    {
        int householdId;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CultureLinkCrmDbContext>();
            var household = new Household { HouseholdName = "Test Household For Deletion" };
            db.Households.Add(household);
            await db.SaveChangesAsync();

            db.People.Add(new Person { FirstName = "Test", LastName = "Member", HouseholdId = household.Id });
            await db.SaveChangesAsync();

            householdId = household.Id;
        }

        var adminClient = await TestAuth.LoginAsync(factory, CrmWebApplicationFactory.SeedAdminEmail, CrmWebApplicationFactory.SeedAdminPassword);
        var token = await TestAuth.GetAntiForgeryTokenAsync(adminClient, $"/Household/Delete/{householdId}");

        var response = await adminClient.PostAsync($"/Household/Delete/{householdId}", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = token
        }));

        Assert.Equal(System.Net.HttpStatusCode.Redirect, response.StatusCode);

        using var verifyScope = factory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<CultureLinkCrmDbContext>();
        var stillExists = await verifyDb.Households.AnyAsync(h => h.Id == householdId);
        Assert.True(stillExists, "Household should not have been deleted while it still has a member attached.");
    }
}
