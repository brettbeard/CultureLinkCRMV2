namespace CultureLinkCRM.Tests.Fixtures;

public static class TestAuth
{
    public static async Task<HttpClient> LoginAsync(CrmWebApplicationFactory factory, string email, string password)
    {
        var client = factory.CreateClient(new() { AllowAutoRedirect = false });

        var loginPage = await client.GetStringAsync("/Account/Login");
        var token = AntiForgeryHelper.ExtractToken(loginPage);

        var form = new Dictionary<string, string>
        {
            ["Email"] = email,
            ["Password"] = password,
            ["__RequestVerificationToken"] = token
        };

        var response = await client.PostAsync("/Account/Login", new FormUrlEncodedContent(form));
        if ((int)response.StatusCode is not (302 or 200))
        {
            throw new InvalidOperationException($"Login failed with status {response.StatusCode}");
        }

        return client;
    }

    public static async Task<string> GetAntiForgeryTokenAsync(HttpClient client, string getUrl)
    {
        var page = await client.GetStringAsync(getUrl);
        return AntiForgeryHelper.ExtractToken(page);
    }
}
