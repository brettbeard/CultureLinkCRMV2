using CultureLinkCRM.Core.Entities;
using CultureLinkCRM.Core.Interfaces;
using CultureLinkCRM.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CultureLinkCRM.Web.Controllers;

[Authorize(Roles = "Admin,Superuser")]
public class CurriculumOrderController(ICurriculumOrderService curriculumOrderService, IOrganizationService organizationService) : Controller
{
    public async Task<IActionResult> Create(int? personId, int? organizationId, CancellationToken ct)
    {
        await PopulateLinkedOrganizationOptions(ct);
        return View(new CurriculumOrderFormViewModel { PersonId = personId, OrganizationId = organizationId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CurriculumOrderFormViewModel vm, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            await PopulateLinkedOrganizationOptions(ct);
            return View(vm);
        }

        var order = new CurriculumOrder
        {
            PersonId = vm.PersonId,
            OrganizationId = vm.OrganizationId,
            Quantity = vm.Quantity,
            OrderDate = vm.OrderDate,
            LinkedOrganizationId = vm.LinkedOrganizationId
        };

        var result = await curriculumOrderService.CreateAsync(order, ct);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            await PopulateLinkedOrganizationOptions(ct);
            return View(vm);
        }

        return RedirectToContact(vm.PersonId, vm.OrganizationId);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, int? personId, int? organizationId, CancellationToken ct)
    {
        await curriculumOrderService.DeleteAsync(id, ct);
        return RedirectToContact(personId, organizationId);
    }

    private async Task PopulateLinkedOrganizationOptions(CancellationToken ct)
    {
        var organizations = await organizationService.SearchAsync(new() { PageSize = 1000 }, ct);
        ViewBag.LinkedOrganizationOptions = organizations.Items.Select(o => new SelectListItem(o.Name, o.Id.ToString())).ToList();
    }

    private IActionResult RedirectToContact(int? personId, int? organizationId) =>
        personId is int pId
            ? RedirectToAction("Details", "Person", new { id = pId })
            : RedirectToAction("Details", "Organization", new { id = organizationId });
}
