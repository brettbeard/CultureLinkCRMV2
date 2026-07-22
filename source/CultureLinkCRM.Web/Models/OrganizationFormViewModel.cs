using System.ComponentModel.DataAnnotations;
using CultureLinkCRM.Core.Enums;

namespace CultureLinkCRM.Web.Models;

public class OrganizationFormViewModel
{
    public int Id { get; set; }

    [Required, StringLength(300)]
    public string Name { get; set; } = string.Empty;

    public OrganizationType OrganizationType { get; set; }

    public List<AddressRowViewModel> Addresses { get; set; } = [];
    public List<PhoneRowViewModel> Phones { get; set; } = [];
    public List<EmailRowViewModel> Emails { get; set; } = [];
}
