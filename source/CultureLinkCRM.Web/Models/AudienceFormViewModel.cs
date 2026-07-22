using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CultureLinkCRM.Web.Models;

public class AudienceFormViewModel
{
    [Required, StringLength(300)]
    public string Name { get; set; } = string.Empty;

    public List<int> SegmentIds { get; set; } = [];

    public List<SelectListItem> SegmentOptions { get; set; } = [];
}
