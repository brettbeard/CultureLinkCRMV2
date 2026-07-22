using CultureLinkCRM.Infrastructure.Data;
using CultureLinkCRM.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CultureLinkCRM.Tests.Integration;

/// <summary>Ref: FR-1 acceptance criteria — an invalid email format is rejected server-side and the record is not saved.</summary>
public class PersonValidationTests(CrmWebApplicationFactory factory) : IClassFixture<CrmWebApplicationFactory>
{
    [Fact]
    public async Task InvalidEmailFormat_IsRejectedAndNotSaved()
    {
        var adminClient = await TestAuth.LoginAsync(factory, CrmWebApplicationFactory.SeedAdminEmail, CrmWebApplicationFactory.SeedAdminPassword);
        var token = await TestAuth.GetAntiForgeryTokenAsync(adminClient, "/Person/Create");

        const string uniqueLastName = "InvalidEmailTestSubject";
        var form = new Dictionary<string, string>
        {
            ["FirstName"] = "Test",
            ["LastName"] = uniqueLastName,
            ["__RequestVerificationToken"] = token,
            ["Emails[0].Type"] = "Home",
            ["Emails[0].Address"] = "not-a-valid-email"
        };

        var response = await adminClient.PostAsync("/Person/Create", new FormUrlEncodedContent(form));

        // Validation failure re-renders the form (200), it does not redirect to Details (302).
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CultureLinkCrmDbContext>();
        var exists = await db.People.AnyAsync(p => p.LastName == uniqueLastName);
        Assert.False(exists, "Person with an invalid email should not have been saved.");
    }
}
