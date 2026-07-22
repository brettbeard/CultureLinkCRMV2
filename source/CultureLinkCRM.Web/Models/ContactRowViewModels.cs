using CultureLinkCRM.Core.Enums;

namespace CultureLinkCRM.Web.Models;

/// <summary>
/// Fixed-slot contact rows for Create/Edit forms (Ref: FR-1, FR-2, FR-3: "one or more" addresses/phones/emails).
/// A small fixed number of slots avoids requiring client-side JS for dynamic add/remove rows in this v1 admin tool;
/// blank rows are simply ignored on save. (No direct SRS requirement — UI simplification.)
/// </summary>
public class AddressRowViewModel
{
    public AddressType Type { get; set; }
    public bool IsPrimary { get; set; }
    public string? Street1 { get; set; }
    public string? Street2 { get; set; }
    public string? City { get; set; }
    public string? StateProvince { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }

    public bool IsBlank => string.IsNullOrWhiteSpace(Street1) && string.IsNullOrWhiteSpace(City);
}

public class PhoneRowViewModel
{
    public PhoneType Type { get; set; }
    public bool IsPrimary { get; set; }
    public string? Number { get; set; }

    public bool IsBlank => string.IsNullOrWhiteSpace(Number);
}

public class EmailRowViewModel
{
    public EmailType Type { get; set; }
    public bool IsPrimary { get; set; }
    public string? Address { get; set; }

    public bool IsBlank => string.IsNullOrWhiteSpace(Address);
}
