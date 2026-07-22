using CultureLinkCRM.Core.Dtos;
using CultureLinkCRM.Core.Entities;

namespace CultureLinkCRM.Infrastructure.Services;

/// <summary>
/// Shared Household-level de-duplication (Ref: FR-10, FR-11): builds one row per matching Household (using
/// HouseholdName + Household contact info) plus one row per Person with no Household, plus one row per Organization.
/// Used identically by the on-screen Audience view and every export path so results always match.
/// </summary>
public static class HouseholdDedupBuilder
{
    public static List<AudienceMemberRow> BuildRows(IEnumerable<Person> people, IEnumerable<Organization> organizations)
    {
        var rows = new List<AudienceMemberRow>();

        var withHousehold = people.Where(p => p.Household is not null).GroupBy(p => p.Household!.Id);
        foreach (var group in withHousehold)
        {
            var household = group.First().Household!;
            var address = household.Addresses.FirstOrDefault(a => a.IsPrimary) ?? household.Addresses.FirstOrDefault();
            var phone = household.Phones.FirstOrDefault(p => p.IsPrimary) ?? household.Phones.FirstOrDefault();
            var email = household.Emails.FirstOrDefault(e => e.IsPrimary) ?? household.Emails.FirstOrDefault();

            rows.Add(new AudienceMemberRow
            {
                Kind = AudienceRowKind.Household,
                DisplayName = household.HouseholdName,
                Street1 = address?.Street1,
                City = address?.City,
                StateProvince = address?.StateProvince,
                PostalCode = address?.PostalCode,
                Country = address?.Country,
                Phone = phone?.Number,
                Email = email?.Address,
                SourceId = household.Id
            });
        }

        foreach (var person in people.Where(p => p.Household is null))
        {
            var address = person.Addresses.FirstOrDefault(a => a.IsPrimary) ?? person.Addresses.FirstOrDefault();
            var phone = person.Phones.FirstOrDefault(p => p.IsPrimary) ?? person.Phones.FirstOrDefault();
            var email = person.Emails.FirstOrDefault(e => e.IsPrimary) ?? person.Emails.FirstOrDefault();

            rows.Add(new AudienceMemberRow
            {
                Kind = AudienceRowKind.Person,
                DisplayName = person.FullName,
                Street1 = address?.Street1,
                City = address?.City,
                StateProvince = address?.StateProvince,
                PostalCode = address?.PostalCode,
                Country = address?.Country,
                Phone = phone?.Number,
                Email = email?.Address,
                SourceId = person.Id
            });
        }

        foreach (var organization in organizations)
        {
            var address = organization.Addresses.FirstOrDefault(a => a.IsPrimary) ?? organization.Addresses.FirstOrDefault();
            var phone = organization.Phones.FirstOrDefault(p => p.IsPrimary) ?? organization.Phones.FirstOrDefault();
            var email = organization.Emails.FirstOrDefault(e => e.IsPrimary) ?? organization.Emails.FirstOrDefault();

            rows.Add(new AudienceMemberRow
            {
                Kind = AudienceRowKind.Organization,
                DisplayName = organization.Name,
                Street1 = address?.Street1,
                City = address?.City,
                StateProvince = address?.StateProvince,
                PostalCode = address?.PostalCode,
                Country = address?.Country,
                Phone = phone?.Number,
                Email = email?.Address,
                SourceId = organization.Id
            });
        }

        return [.. rows.OrderBy(r => r.DisplayName)];
    }
}
