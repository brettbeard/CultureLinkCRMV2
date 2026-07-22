using System.Text.RegularExpressions;

namespace CultureLinkCRM.Tests.Fixtures;

public static partial class AntiForgeryHelper
{
    public static string ExtractToken(string html)
    {
        var match = TokenRegex().Match(html);
        if (!match.Success)
        {
            throw new InvalidOperationException("Anti-forgery token not found in response HTML.");
        }
        return match.Groups[1].Value;
    }

    [GeneratedRegex("__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"")]
    private static partial Regex TokenRegex();
}
