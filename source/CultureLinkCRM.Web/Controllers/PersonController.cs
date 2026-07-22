using CultureLinkCRM.Core.Dtos;
using CultureLinkCRM.Core.Entities;
using CultureLinkCRM.Core.Interfaces;
using CultureLinkCRM.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CultureLinkCRM.Web.Controllers;

[Authorize]
public class PersonController(
    IPersonService personService,
    IHouseholdService householdService,
    ISegmentService segmentService,
    INetworkService networkService,
    IDonationService donationService,
    IDonorStatusService donorStatusService,
    ISeminarService seminarService,
    ICurriculumOrderService curriculumOrderService,
    IEngagementService engagementService) : Controller
{
    public async Task<IActionResult> Index(PersonFilter filter, CancellationToken ct)
    {
        var results = await personService.SearchAsync(filter, ct);
        var vm = new PersonIndexViewModel
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
        var person = await personService.GetByIdAsync(id, ct);
        if (person is null) return NotFound();

        ViewBag.EffectiveContact = await personService.GetEffectiveContactInfoAsync(id, ct);
        ViewBag.DonorStatus = await donorStatusService.GetDonorStatusForPersonAsync(id, ct);
        ViewBag.Donations = await donationService.GetForPersonAsync(id, ct);
        ViewBag.CurriculumOrders = await curriculumOrderService.GetForPersonAsync(id, ct);
        ViewBag.Engagements = await engagementService.GetForPersonAsync(id, ct);
        ViewBag.SeminarAttendances = await seminarService.GetAttendanceForPersonAsync(id, ct);
        return View(person);
    }

    [Authorize(Roles = "Admin,Superuser")]
    public async Task<IActionResult> Create(CancellationToken ct)
    {
        var vm = new PersonFormViewModel();
        await PopulateHouseholdOptions(vm, ct);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Superuser")]
    public async Task<IActionResult> Create(PersonFormViewModel vm, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            await PopulateHouseholdOptions(vm, ct);
            return View(vm);
        }

        var person = MapToEntity(vm, new Person());
        var result = await personService.CreateAsync(person, ct);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            await PopulateHouseholdOptions(vm, ct);
            return View(vm);
        }

        return RedirectToAction(nameof(Details), new { id = result.Value!.Id });
    }

    [Authorize(Roles = "Admin,Superuser")]
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var person = await personService.GetByIdAsync(id, ct);
        if (person is null) return NotFound();

        var vm = new PersonFormViewModel
        {
            Id = person.Id,
            FirstName = person.FirstName,
            LastName = person.LastName,
            MiddleName = person.MiddleName,
            Suffix = person.Suffix,
            HouseholdId = person.HouseholdId,
            Addresses = ContactRowMapper.ToAddressRows(person.Addresses, a => (a.Type, a.IsPrimary, a.Street1, a.Street2, a.City, a.StateProvince, a.PostalCode, a.Country)),
            Phones = ContactRowMapper.ToPhoneRows(person.Phones, p => (p.Type, p.IsPrimary, p.Number)),
            Emails = ContactRowMapper.ToEmailRows(person.Emails, e => (e.Type, e.IsPrimary, e.Address))
        };
        await PopulateHouseholdOptions(vm, ct);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Superuser")]
    public async Task<IActionResult> Edit(int id, PersonFormViewModel vm, CancellationToken ct)
    {
        if (id != vm.Id) return BadRequest();

        if (!ModelState.IsValid)
        {
            await PopulateHouseholdOptions(vm, ct);
            return View(vm);
        }

        var person = MapToEntity(vm, new Person { Id = id });
        var result = await personService.UpdateAsync(person, ct);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            await PopulateHouseholdOptions(vm, ct);
            return View(vm);
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize(Roles = "Admin,Superuser")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var person = await personService.GetByIdAsync(id, ct);
        if (person is null) return NotFound();
        return View(person);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Superuser")]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken ct)
    {
        var result = await personService.DeleteAsync(id, ct);
        if (!result.Succeeded)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction(nameof(Delete), new { id });
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateHouseholdOptions(PersonFormViewModel vm, CancellationToken ct)
    {
        var households = await householdService.SearchAsync(null, 1, 1000, ct);
        vm.HouseholdOptions = households.Items
            .Select(h => new SelectListItem(h.HouseholdName, h.Id.ToString()))
            .ToList();
    }

    private static Person MapToEntity(PersonFormViewModel vm, Person person)
    {
        person.FirstName = vm.FirstName;
        person.LastName = vm.LastName;
        person.MiddleName = vm.MiddleName;
        person.Suffix = vm.Suffix;
        person.HouseholdId = vm.HouseholdId;

        person.Addresses = [.. vm.Addresses.Where(a => !a.IsBlank).Select(a => new PersonAddress
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

        person.Phones = [.. vm.Phones.Where(p => !p.IsBlank).Select(p => new PersonPhone
        {
            Type = p.Type,
            IsPrimary = p.IsPrimary,
            Number = p.Number ?? string.Empty
        })];

        person.Emails = [.. vm.Emails.Where(e => !e.IsBlank).Select(e => new PersonEmail
        {
            Type = e.Type,
            IsPrimary = e.IsPrimary,
            Address = e.Address ?? string.Empty
        })];

        return person;
    }
}
