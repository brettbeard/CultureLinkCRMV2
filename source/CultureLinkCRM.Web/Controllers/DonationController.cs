using CultureLinkCRM.Core.Entities;
using CultureLinkCRM.Core.Interfaces;
using CultureLinkCRM.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CultureLinkCRM.Web.Controllers;

[Authorize(Roles = "Admin,Superuser")]
public class DonationController(IDonationService donationService) : Controller
{
    public IActionResult Create(int? personId, int? organizationId) =>
        View(new DonationFormViewModel { PersonId = personId, OrganizationId = organizationId });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DonationFormViewModel vm, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(vm);

        var donation = new Donation
        {
            PersonId = vm.PersonId,
            OrganizationId = vm.OrganizationId,
            Amount = vm.Amount,
            DonationDate = vm.DonationDate,
            FundProjectDesignation = vm.FundProjectDesignation
        };

        var result = await donationService.CreateAsync(donation, ct);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            return View(vm);
        }

        return RedirectToContact(vm.PersonId, vm.OrganizationId);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, int? personId, int? organizationId, CancellationToken ct)
    {
        await donationService.DeleteAsync(id, ct);
        return RedirectToContact(personId, organizationId);
    }

    private IActionResult RedirectToContact(int? personId, int? organizationId) =>
        personId is int pId
            ? RedirectToAction("Details", "Person", new { id = pId })
            : RedirectToAction("Details", "Organization", new { id = organizationId });
}
