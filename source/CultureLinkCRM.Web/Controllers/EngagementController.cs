using CultureLinkCRM.Core.Entities;
using CultureLinkCRM.Core.Interfaces;
using CultureLinkCRM.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CultureLinkCRM.Web.Controllers;

[Authorize(Roles = "Admin,Superuser")]
public class EngagementController(IEngagementService engagementService) : Controller
{
    public async Task<IActionResult> Create(int? personId, int? organizationId, CancellationToken ct)
    {
        await PopulateTypeOptions(ct);
        return View(new EngagementFormViewModel { PersonId = personId, OrganizationId = organizationId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EngagementFormViewModel vm, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            await PopulateTypeOptions(ct);
            return View(vm);
        }

        var engagement = new Engagement
        {
            PersonId = vm.PersonId,
            OrganizationId = vm.OrganizationId,
            EngagementTypeId = vm.EngagementTypeId,
            StartDate = vm.StartDate,
            EndDate = vm.EndDate,
            Notes = vm.Notes
        };

        var result = await engagementService.CreateAsync(engagement, ct);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            await PopulateTypeOptions(ct);
            return View(vm);
        }

        return RedirectToContact(vm.PersonId, vm.OrganizationId);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, int? personId, int? organizationId, CancellationToken ct)
    {
        await engagementService.DeleteAsync(id, ct);
        return RedirectToContact(personId, organizationId);
    }

    private async Task PopulateTypeOptions(CancellationToken ct)
    {
        var types = await engagementService.GetTypesAsync(ct);
        ViewBag.TypeOptions = types.Select(t => new SelectListItem(t.Name, t.Id.ToString())).ToList();
    }

    private IActionResult RedirectToContact(int? personId, int? organizationId) =>
        personId is int pId
            ? RedirectToAction("Details", "Person", new { id = pId })
            : RedirectToAction("Details", "Organization", new { id = organizationId });
}
