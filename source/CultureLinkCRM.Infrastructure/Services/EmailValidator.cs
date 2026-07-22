using System.Net.Mail;

namespace CultureLinkCRM.Infrastructure.Services;

public static class EmailValidator
{
    public static bool IsValid(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
        {
            return false;
        }

        try
        {
            var parsed = new MailAddress(address);
            return parsed.Address == address;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
