using CultureLinkCRM.Core.Entities;
using CultureLinkCRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CultureLinkCRM.Web.Controllers;

[Authorize]
public class NetworkController(INetworkService networkService) : Controller
{
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var networks = await networkService.GetAllAsync(ct);
        return View(networks);
    }

    public async Task<IActionResult> Details(int id, CancellationToken ct)
    {
        var network = await networkService.GetByIdAsync(id, ct);
        if (network is null) return NotFound();
        return View(network);
    }

    [Authorize(Roles = "Admin,Superuser")]
    public async Task<IActionResult> Create(CancellationToken ct)
    {
        await PopulateParentOptions(null, ct);
        return View(new Network());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Superuser")]
    public async Task<IActionResult> Create(Network network, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            await PopulateParentOptions(null, ct);
            return View(network);
        }

        var result = await networkService.CreateAsync(network, ct);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            await PopulateParentOptions(null, ct);
            return View(network);
        }

        return RedirectToAction(nameof(Details), new { id = result.Value!.Id });
    }

    [Authorize(Roles = "Admin,Superuser")]
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var network = await networkService.GetByIdAsync(id, ct);
        if (network is null) return NotFound();
        await PopulateParentOptions(id, ct);
        return View(network);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Superuser")]
    public async Task<IActionResult> Edit(int id, Network network, CancellationToken ct)
    {
        if (id != network.Id) return BadRequest();
        if (!ModelState.IsValid)
        {
            await PopulateParentOptions(id, ct);
            return View(network);
        }

        var result = await networkService.UpdateAsync(network, ct);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            await PopulateParentOptions(id, ct);
            return View(network);
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize(Roles = "Admin,Superuser")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var network = await networkService.GetByIdAsync(id, ct);
        if (network is null) return NotFound();
        return View(network);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Superuser")]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken ct)
    {
        var result = await networkService.DeleteAsync(id, ct);
        if (!result.Succeeded)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction(nameof(Delete), new { id });
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateParentOptions(int? excludeId, CancellationToken ct)
    {
        var networks = await networkService.GetAllAsync(ct);
        ViewBag.ParentOptions = networks
            .Where(n => n.Id != excludeId)
            .Select(n => new SelectListItem(n.Name, n.Id.ToString()))
            .ToList();
    }
}
