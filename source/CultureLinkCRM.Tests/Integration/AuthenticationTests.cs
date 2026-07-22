using CultureLinkCRM.Tests.Fixtures;

namespace CultureLinkCRM.Tests.Integration;

/// <summary>Ref: FR-12 acceptance criteria — valid credentials authenticate and establish a session; invalid credentials are rejected.</summary>
public class AuthenticationTests(CrmWebApplicationFactory factory) : IClassFixture<CrmWebApplicationFactory>
{
    [Fact]
    public async Task ValidCredentials_LogsInAndRedirectsToDashboard()
    {
        var client = factory.CreateClient(new() { AllowAutoRedirect = false });
        var token = await TestAuth.GetAntiForgeryTokenAsync(client, "/Account/Login");

        var response = await client.PostAsync("/Account/Login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Email"] = CrmWebApplicationFactory.SeedAdminEmail,
            ["Password"] = CrmWebApplicationFactory.SeedAdminPassword,
            ["__RequestVerificationToken"] = token
        }));

        Assert.Equal(System.Net.HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/", response.Headers.Location!.ToString());
    }

    [Fact]
    public async Task InvalidCredentials_DoesNotAuthenticate()
    {
        var client = factory.CreateClient(new() { AllowAutoRedirect = false });
        var token = await TestAuth.GetAntiForgeryTokenAsync(client, "/Account/Login");

        var response = await client.PostAsync("/Account/Login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Email"] = CrmWebApplicationFactory.SeedAdminEmail,
            ["Password"] = "WrongPassword!",
            ["__RequestVerificationToken"] = token
        }));

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);

        var protectedPage = await client.GetAsync("/Person");
        Assert.Equal(System.Net.HttpStatusCode.Redirect, protectedPage.StatusCode);
        Assert.Contains("/Account/Login", protectedPage.Headers.Location!.ToString());
    }
}
