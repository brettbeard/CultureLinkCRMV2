using CultureLinkCRM.Core.Interfaces;
using CultureLinkCRM.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CultureLinkCRM.Web.Controllers;

// Available to all roles including read-only User (Ref: FR-13: Audience Builder is explicitly granted to User).
[Authorize]
public class AudienceController(IAudienceService audienceService, ISegmentService segmentService) : Controller
{
    public async Task<IActionResult> Index(CancellationToken ct) => View(await audienceService.GetAllAsync(ct));

    public async Task<IActionResult> Details(int id, CancellationToken ct)
    {
        var audience = await audienceService.GetByIdAsync(id, ct);
        if (audience is null) return NotFound();

        ViewBag.Members = await audienceService.GetMembersAsync(id, ct);
        return View(audience);
    }

    public async Task<IActionResult> Create(CancellationToken ct)
    {
        var vm = new AudienceFormViewModel();
        await PopulateSegmentOptions(vm, ct);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AudienceFormViewModel vm, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            await PopulateSegmentOptions(vm, ct);
            return View(vm);
        }

        var result = await audienceService.CreateAsync(vm.Name, vm.SegmentIds, ct);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            await PopulateSegmentOptions(vm, ct);
            return View(vm);
        }

        return RedirectToAction(nameof(Details), new { id = result.Value!.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await audienceService.DeleteAsync(id, ct);
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateSegmentOptions(AudienceFormViewModel vm, CancellationToken ct)
    {
        var segments = await segmentService.GetAllAsync(includeComputed: true, ct);
        vm.SegmentOptions = segments.Select(s => new SelectListItem(s.Name, s.Id.ToString())).ToList();
    }
}
