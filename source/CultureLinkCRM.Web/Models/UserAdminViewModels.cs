using System.ComponentModel.DataAnnotations;
using CultureLinkCRM.Core.Enums;

namespace CultureLinkCRM.Web.Models;

public class CreateUserViewModel
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, DataType(DataType.Password), MinLength(8)]
    public string Password { get; set; } = string.Empty;

    public UserRole Role { get; set; }
}

public class UpdateUserRoleViewModel
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; }
}
