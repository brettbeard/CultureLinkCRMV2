namespace CultureLinkCRM.Web.Models;

/// <summary>Converts between the fixed-slot contact row view models and Core contact entities, skipping blank rows.</summary>
public static class ContactRowMapper
{
    public const int SlotCount = 3;

    public static List<AddressRowViewModel> ToAddressRows<TEntity>(IEnumerable<TEntity> entities, Func<TEntity, (CultureLinkCRM.Core.Enums.AddressType Type, bool IsPrimary, string Street1, string? Street2, string City, string StateProvince, string PostalCode, string Country)> select)
    {
        var rows = entities.Select(select)
            .Select(a => new AddressRowViewModel
            {
                Type = a.Type,
                IsPrimary = a.IsPrimary,
                Street1 = a.Street1,
                Street2 = a.Street2,
                City = a.City,
                StateProvince = a.StateProvince,
                PostalCode = a.PostalCode,
                Country = a.Country
            }).ToList();

        while (rows.Count < SlotCount) rows.Add(new AddressRowViewModel());
        return rows;
    }

    public static List<PhoneRowViewModel> ToPhoneRows<TEntity>(IEnumerable<TEntity> entities, Func<TEntity, (CultureLinkCRM.Core.Enums.PhoneType Type, bool IsPrimary, string Number)> select)
    {
        var rows = entities.Select(select)
            .Select(p => new PhoneRowViewModel { Type = p.Type, IsPrimary = p.IsPrimary, Number = p.Number })
            .ToList();

        while (rows.Count < SlotCount) rows.Add(new PhoneRowViewModel());
        return rows;
    }

    public static List<EmailRowViewModel> ToEmailRows<TEntity>(IEnumerable<TEntity> entities, Func<TEntity, (CultureLinkCRM.Core.Enums.EmailType Type, bool IsPrimary, string Address)> select)
    {
        var rows = entities.Select(select)
            .Select(e => new EmailRowViewModel { Type = e.Type, IsPrimary = e.IsPrimary, Address = e.Address })
            .ToList();

        while (rows.Count < SlotCount) rows.Add(new EmailRowViewModel());
        return rows;
    }
}
