using CultureLinkCRM.Core.Entities;

namespace CultureLinkCRM.Infrastructure.Services;

/// <summary>Shared validation for the Person/Household/Organization contact-info collections (Ref: FR-1, FR-2, FR-3): email format, international phone format, and at most one IsPrimary per type.</summary>
public static class ContactValidation
{
    public static string? ValidateEmails(IEnumerable<string> addresses)
    {
        foreach (var address in addresses)
        {
            if (!EmailValidator.IsValid(address))
            {
                return $"'{address}' is not a valid email address.";
            }
        }
        return null;
    }

    public static string? ValidatePhones(IEnumerable<string> numbers)
    {
        foreach (var number in numbers)
        {
            if (!PhoneNumberValidator.IsValid(number))
            {
                return $"'{number}' is not a valid phone number.";
            }
        }
        return null;
    }

    public static string? ValidatePrimaryFlags<TEnum>(IEnumerable<(TEnum Type, bool IsPrimary)> entries, string entryKind) where TEnum : notnull
    {
        var duplicate = entries
            .Where(e => e.IsPrimary)
            .GroupBy(e => e.Type)
            .FirstOrDefault(g => g.Count() > 1);

        return duplicate is null
            ? null
            : $"Only one {entryKind} of type '{duplicate.Key}' may be marked primary.";
    }
}
