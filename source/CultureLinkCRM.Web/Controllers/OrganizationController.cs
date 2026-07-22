using CultureLinkCRM.Core.Dtos;
using CultureLinkCRM.Core.Entities;
using CultureLinkCRM.Core.Interfaces;
using CultureLinkCRM.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CultureLinkCRM.Web.Controllers;

[Authorize]
public class OrganizationController(
    IOrganizationService organizationService,
    ISegmentService segmentService,
    INetworkService networkService,
    IDonationService donationService,
    IDonorStatusService donorStatusService,
    ICurriculumOrderService curriculumOrderService,
    IEngagementService engagementService) : Controller
{
    public async Task<IActionResult> Index(OrganizationFilter filter, CancellationToken ct)
    {
        var results = await organizationService.SearchAsync(filter, ct);
        var vm = new OrganizationIndexViewModel
        {
            Results = results,
            Filter = filter,
            SegmentOptions = (await segmentService.GetAllAsync(includeComputed: true, ct))
                .Select(s => new SelectListItem(s.Name, s.Id.ToString())).ToList(),
            NetworkOptions = (await networkService.GetAllAsync(ct))
                .Select(n => new SelectListItem(n.Name, n.Id.ToString())).ToList()
        };
        return View(vm);
    }

    public async Task<IActionResult> Details(int id, CancellationToken ct)
    {
        var organization = await organizationService.GetByIdAsync(id, ct);
        if (organization is null) return NotFound();

        ViewBag.DonorStatus = await donorStatusService.GetDonorStatusForOrganizationAsync(id, ct);
        ViewBag.Donations = await donationService.GetForOrganizationAsync(id, ct);
        ViewBag.CurriculumOrders = await curriculumOrderService.GetForOrganizationAsync(id, ct);
        ViewBag.Engagements = await engagementService.GetForOrganizationAsync(id, ct);
        return View(organization);
    }

    [Authorize(Roles = "Admin,Superuser")]
    public IActionResult Create() => View(new OrganizationFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Superuser")]
    public async Task<IActionResult> Create(OrganizationFormViewModel vm, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(vm);

        var organization = MapToEntity(vm, new Organization());
        var result = await organizationService.CreateAsync(organization, ct);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            return View(vm);
        }

        return RedirectToAction(nameof(Details), new { id = result.Value!.Id });
    }

    [Authorize(Roles = "Admin,Superuser")]
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var organization = await organizationService.GetByIdAsync(id, ct);
        if (organization is null) return NotFound();

        var vm = new OrganizationFormViewModel
        {
            Id = organization.Id,
            Name = organization.Name,
            OrganizationType = organization.OrganizationType,
            Addresses = ContactRowMapper.ToAddressRows(organization.Addresses, a => (a.Type, a.IsPrimary, a.Street1, a.Street2, a.City, a.StateProvince, a.PostalCode, a.Country)),
            Phones = ContactRowMapper.ToPhoneRows(organization.Phones, p => (p.Type, p.IsPrimary, p.Number)),
            Emails = ContactRowMapper.ToEmailRows(organization.Emails, e => (e.Type, e.IsPrimary, e.Address))
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Superuser")]
    public async Task<IActionResult> Edit(int id, OrganizationFormViewModel vm, CancellationToken ct)
    {
        if (id != vm.Id) return BadRequest();
        if (!ModelState.IsValid) return View(vm);

        var organization = MapToEntity(vm, new Organization { Id = id });
        var result = await organizationService.UpdateAsync(organization, ct);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            return View(vm);
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize(Roles = "Admin,Superuser")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var organization = await organizationService.GetByIdAsync(id, ct);
        if (organization is null) return NotFound();
        return View(organization);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Superuser")]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken ct)
    {
        var result = await organizationService.DeleteAsync(id, ct);
        if (!result.Succeeded)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction(nameof(Delete), new { id });
        }

        return RedirectToAction(nameof(Index));
    }

    private static Organization MapToEntity(OrganizationFormViewModel vm, Organization organization)
    {
        organization.Name = vm.Name;
        organization.OrganizationType = vm.OrganizationType;

        organization.Addresses = [.. vm.Addresses.Where(a => !a.IsBlank).Select(a => new OrganizationAddress
        {
            Type = a.Type,
            IsPrimary = a.IsPrimary,
            Street1 = a.Street1 ?? string.Empty,
            Street2 = a.Street2,
            City = a.City ?? string.Empty,
            StateProvince = a.StateProvince ?? string.Empty,
            PostalCode = a.PostalCode ?? string.Empty,
            Country = a.Country ?? string.Empty
        })];

        organization.Phones = [.. vm.Phones.Where(p => !p.IsBlank).Select(p => new OrganizationPhone
        {
            Type = p.Type,
            IsPrimary = p.IsPrimary,
            Number = p.Number ?? string.Empty
        })];

        organization.Emails = [.. vm.Emails.Where(e => !e.IsBlank).Select(e => new OrganizationEmail
        {
            Type = e.Type,
            IsPrimary = e.IsPrimary,
            Address = e.Address ?? string.Empty
        })];

        return organization;
    }
}
