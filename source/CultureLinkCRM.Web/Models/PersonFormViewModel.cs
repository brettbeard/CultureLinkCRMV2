using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CultureLinkCRM.Web.Models;

public class PersonFormViewModel
{
    public int Id { get; set; }

    [Required, StringLength(200)]
    public string FirstName { get; set; } = string.Empty;

    [Required, StringLength(200)]
    public string LastName { get; set; } = string.Empty;

    [StringLength(200)]
    public string? MiddleName { get; set; }

    [StringLength(50)]
    public string? Suffix { get; set; }

    public int? HouseholdId { get; set; }

    public List<AddressRowViewModel> Addresses { get; set; } = [];
    public List<PhoneRowViewModel> Phones { get; set; } = [];
    public List<EmailRowViewModel> Emails { get; set; } = [];

    public List<SelectListItem> HouseholdOptions { get; set; } = [];
}
