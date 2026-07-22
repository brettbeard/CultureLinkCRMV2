using CultureLinkCRM.Core.Dtos;
using CultureLinkCRM.Core.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CultureLinkCRM.Web.Models;

public class PersonIndexViewModel
{
    public required PagedResult<Person> Results { get; init; }
    public required PersonFilter Filter { get; init; }
    public List<SelectListItem> SegmentOptions { get; init; } = [];
    public List<SelectListItem> NetworkOptions { get; init; } = [];
}
