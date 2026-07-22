using CultureLinkCRM.Core.Dtos;
using CultureLinkCRM.Core.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CultureLinkCRM.Web.Models;

public class OrganizationIndexViewModel
{
    public required PagedResult<Organization> Results { get; init; }
    public required OrganizationFilter Filter { get; init; }
    public List<SelectListItem> SegmentOptions { get; init; } = [];
    public List<SelectListItem> NetworkOptions { get; init; } = [];
}
