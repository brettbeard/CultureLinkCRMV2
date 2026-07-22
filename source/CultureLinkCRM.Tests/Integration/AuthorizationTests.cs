using CultureLinkCRM.Tests.Fixtures;

namespace CultureLinkCRM.Tests.Integration;

/// <summary>Ref: FR-13 acceptance criteria — a non-Admin user hitting an Admin-only URL directly is denied, with no data exposure.</summary>
public class AuthorizationTests(CrmWebApplicationFactory factory) : IClassFixture<CrmWebApplicationFactory>
{
    private const string UserEmail = "readonly-user@test.local";
    private const string UserPassword = "ReadOnly123!";

    [Fact]
    public async Task UserRole_DirectUrlToUserManagement_IsDenied()
    {
        var adminClient = await TestAuth.LoginAsync(factory, CrmWebApplicationFactory.SeedAdminEmail, CrmWebApplicationFactory.SeedAdminPassword);
        await EnsureUserRoleAccountExistsAsync(adminClient);

        var userClient = await TestAuth.LoginAsync(factory, UserEmail, UserPassword);

        var response = await userClient.GetAsync("/UserAdmin");
        Assert.Equal(System.Net.HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("AccessDenied", response.Headers.Location!.ToString());
    }

    [Fact]
    public async Task AdminRole_CanAccessUserManagement()
    {
        var adminClient = await TestAuth.LoginAsync(factory, CrmWebApplicationFactory.SeedAdminEmail, CrmWebApplicationFactory.SeedAdminPassword);

        var response = await adminClient.GetAsync("/UserAdmin");
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    }

    private static async Task EnsureUserRoleAccountExistsAsync(HttpClient adminClient)
    {
        var token = await TestAuth.GetAntiForgeryTokenAsync(adminClient, "/UserAdmin/Create");
        await adminClient.PostAsync("/UserAdmin/Create", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Email"] = UserEmail,
            ["Password"] = UserPassword,
            ["Role"] = "User",
            ["__RequestVerificationToken"] = token
        }));
    }
}
