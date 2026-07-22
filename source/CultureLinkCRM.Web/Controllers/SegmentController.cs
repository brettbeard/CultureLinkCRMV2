using CultureLinkCRM.Core.Dtos;
using CultureLinkCRM.Core.Entities;
using CultureLinkCRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CultureLinkCRM.Web.Controllers;

[Authorize]
public class SegmentController(ISegmentService segmentService, IPersonService personService, IOrganizationService organizationService) : Controller
{
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var segments = await segmentService.GetAllAsync(includeComputed: true, ct);
        return View(segments);
    }

    public async Task<IActionResult> Details(int id, CancellationToken ct)
    {
        var segment = await segmentService.GetByIdAsync(id, ct);
        if (segment is null) return NotFound();

        if (!segment.IsComputed)
        {
            var people = await personService.SearchAsync(new PersonFilter { PageSize = 1000 }, ct);
            var organizations = await organizationService.SearchAsync(new OrganizationFilter { PageSize = 1000 }, ct);
            ViewBag.PersonOptions = people.Items.Select(p => new SelectListItem(p.FullName, p.Id.ToString())).ToList();
            ViewBag.OrganizationOptions = organizations.Items.Select(o => new SelectListItem(o.Name, o.Id.ToString())).ToList();
        }

        return View(segment);
    }

    [Authorize(Roles = "Admin,Superuser")]
    public IActionResult Create() => View(new Segment());

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Superuser")]
    public async Task<IActionResult> Create(Segment segment, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(segment);

        var result = await segmentService.CreateAsync(segment, ct);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            return View(segment);
        }

        return RedirectToAction(nameof(Details), new { id = result.Value!.Id });
    }

    [Authorize(Roles = "Admin,Superuser")]
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var segment = await segmentService.GetByIdAsync(id, ct);
        if (segment is null) return NotFound();
        if (segment.IsComputed) return Forbid();
        return View(segment);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Superuser")]
    public async Task<IActionResult> Edit(int id, Segment segment, CancellationToken ct)
    {
        if (id != segment.Id) return BadRequest();
        if (!ModelState.IsValid) return View(segment);

        var result = await segmentService.UpdateAsync(segment, ct);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            return View(segment);
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize(Roles = "Admin,Superuser")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var segment = await segmentService.GetByIdAsync(id, ct);
        if (segment is null) return NotFound();
        if (segment.IsComputed) return Forbid();
        return View(segment);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Superuser")]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken ct)
    {
        var result = await segmentService.DeleteAsync(id, ct);
        if (!result.Succeeded)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction(nameof(Delete), new { id });
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Superuser")]
    public async Task<IActionResult> Assign(int segmentId, int? personId, int? organizationId, CancellationToken ct)
    {
        var result = await segmentService.AssignAsync(segmentId, personId, organizationId, ct);
        if (!result.Succeeded)
        {
            TempData["Error"] = result.Error;
        }
        return RedirectToAction(nameof(Details), new { id = segmentId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Superuser")]
    public async Task<IActionResult> Unassign(int segmentId, int assignmentId, CancellationToken ct)
    {
        var result = await segmentService.UnassignAsync(assignmentId, ct);
        if (!result.Succeeded)
        {
            TempData["Error"] = result.Error;
        }
        return RedirectToAction(nameof(Details), new { id = segmentId });
    }
}
