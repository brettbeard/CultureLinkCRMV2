using PhoneNumbers;

namespace CultureLinkCRM.Infrastructure.Services;

/// <summary>Wraps libphonenumber-csharp so international phone numbers (Ref: FR-1) are validated, not just US-format regex-matched.</summary>
public static class PhoneNumberValidator
{
    public static bool IsValid(string number)
    {
        if (string.IsNullOrWhiteSpace(number))
        {
            return false;
        }

        try
        {
            var util = PhoneNumberUtil.GetInstance();
            // Default region "US" is used only when the input has no leading "+" country code;
            // international numbers should be entered with a "+" prefix and are parsed regardless of default region.
            var parsed = util.Parse(number, "US");
            return util.IsValidNumber(parsed);
        }
        catch (NumberParseException)
        {
            return false;
        }
    }
}
