using System.ComponentModel.DataAnnotations;
using CultureLinkCRM.Core.Enums;

namespace CultureLinkCRM.Web.Models;

public class HouseholdFormViewModel
{
    public int Id { get; set; }

    [Required, StringLength(300)]
    public string HouseholdName { get; set; } = string.Empty;

    public MailPreference MailPreference { get; set; }

    public List<AddressRowViewModel> Addresses { get; set; } = [];
    public List<PhoneRowViewModel> Phones { get; set; } = [];
    public List<EmailRowViewModel> Emails { get; set; } = [];
}
