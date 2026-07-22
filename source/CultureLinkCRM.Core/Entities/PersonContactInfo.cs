using CultureLinkCRM.Core.Enums;

namespace CultureLinkCRM.Core.Entities;

public class PersonAddress
{
    public int Id { get; set; }
    public int PersonId { get; set; }
    public Person? Person { get; set; }
    public AddressType Type { get; set; }
    public bool IsPrimary { get; set; }
    public string Street1 { get; set; } = string.Empty;
    public string? Street2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string StateProvince { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
}

public class PersonPhone
{
    public int Id { get; set; }
    public int PersonId { get; set; }
    public Person? Person { get; set; }
    public PhoneType Type { get; set; }
    public bool IsPrimary { get; set; }
    public string Number { get; set; } = string.Empty;
}

public class PersonEmail
{
    public int Id { get; set; }
    public int PersonId { get; set; }
    public Person? Person { get; set; }
    public EmailType Type { get; set; }
    public bool IsPrimary { get; set; }
    public string Address { get; set; } = string.Empty;
}
