using CultureLinkCRM.Core.Entities;
using CultureLinkCRM.Core.Interfaces;
using CultureLinkCRM.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CultureLinkCRM.Web.Controllers;

[Authorize]
public class HouseholdController(IHouseholdService householdService) : Controller
{
    public async Task<IActionResult> Index(string? name, int page = 1, CancellationToken ct = default)
    {
        var results = await householdService.SearchAsync(name, page, 25, ct);
        ViewBag.NameFilter = name;
        return View(results);
    }

    public async Task<IActionResult> Details(int id, CancellationToken ct)
    {
        var household = await householdService.GetByIdAsync(id, ct);
        if (household is null) return NotFound();
        return View(household);
    }

    [Authorize(Roles = "Admin,Superuser")]
    public IActionResult Create() => View(new HouseholdFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Superuser")]
    public async Task<IActionResult> Create(HouseholdFormViewModel vm, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(vm);

        var household = MapToEntity(vm, new Household());
        var result = await householdService.CreateAsync(household, ct);
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
        var household = await householdService.GetByIdAsync(id, ct);
        if (household is null) return NotFound();

        var vm = new HouseholdFormViewModel
        {
            Id = household.Id,
            HouseholdName = household.HouseholdName,
            MailPreference = household.MailPreference,
            Addresses = ContactRowMapper.ToAddressRows(household.Addresses, a => (a.Type, a.IsPrimary, a.Street1, a.Street2, a.City, a.StateProvince, a.PostalCode, a.Country)),
            Phones = ContactRowMapper.ToPhoneRows(household.Phones, p => (p.Type, p.IsPrimary, p.Number)),
            Emails = ContactRowMapper.ToEmailRows(household.Emails, e => (e.Type, e.IsPrimary, e.Address))
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Superuser")]
    public async Task<IActionResult> Edit(int id, HouseholdFormViewModel vm, CancellationToken ct)
    {
        if (id != vm.Id) return BadRequest();
        if (!ModelState.IsValid) return View(vm);

        var household = MapToEntity(vm, new Household { Id = id });
        var result = await householdService.UpdateAsync(household, ct);
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
        var household = await householdService.GetByIdAsync(id, ct);
        if (household is null) return NotFound();
        return View(household);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Superuser")]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken ct)
    {
        var result = await householdService.DeleteAsync(id, ct);
        if (!result.Succeeded)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction(nameof(Delete), new { id });
        }

        return RedirectToAction(nameof(Index));
    }

    private static Household MapToEntity(HouseholdFormViewModel vm, Household household)
    {
        household.HouseholdName = vm.HouseholdName;
        household.MailPreference = vm.MailPreference;

        household.Addresses = [.. vm.Addresses.Where(a => !a.IsBlank).Select(a => new HouseholdAddress
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

        household.Phones = [.. vm.Phones.Where(p => !p.IsBlank).Select(p => new HouseholdPhone
        {
            Type = p.Type,
            IsPrimary = p.IsPrimary,
            Number = p.Number ?? string.Empty
        })];

        household.Emails = [.. vm.Emails.Where(e => !e.IsBlank).Select(e => new HouseholdEmail
        {
            Type = e.Type,
            IsPrimary = e.IsPrimary,
            Address = e.Address ?? string.Empty
        })];

        return household;
    }
}
