using CultureLinkCRM.Core.Entities;
using CultureLinkCRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CultureLinkCRM.Web.Controllers;

[Authorize]
public class SeminarController(ISeminarService seminarService, IPersonService personService) : Controller
{
    public async Task<IActionResult> Index(CancellationToken ct) => View(await seminarService.GetAllAsync(ct));

    public async Task<IActionResult> Details(int id, CancellationToken ct)
    {
        var seminar = await seminarService.GetByIdAsync(id, ct);
        if (seminar is null) return NotFound();

        if (User.IsInRole("Admin") || User.IsInRole("Superuser"))
        {
            var people = await personService.SearchAsync(new() { PageSize = 1000 }, ct);
            ViewBag.PersonOptions = people.Items.Select(p => new SelectListItem(p.FullName, p.Id.ToString())).ToList();
        }

        return View(seminar);
    }

    [Authorize(Roles = "Admin,Superuser")]
    public async Task<IActionResult> Create(CancellationToken ct)
    {
        await PopulateParentOptions(null, ct);
        return View(new Seminar());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Superuser")]
    public async Task<IActionResult> Create(Seminar seminar, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            await PopulateParentOptions(null, ct);
            return View(seminar);
        }

        var result = await seminarService.CreateAsync(seminar, ct);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            await PopulateParentOptions(null, ct);
            return View(seminar);
        }

        return RedirectToAction(nameof(Details), new { id = result.Value!.Id });
    }

    [Authorize(Roles = "Admin,Superuser")]
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var seminar = await seminarService.GetByIdAsync(id, ct);
        if (seminar is null) return NotFound();
        await PopulateParentOptions(id, ct);
        return View(seminar);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Superuser")]
    public async Task<IActionResult> Edit(int id, Seminar seminar, CancellationToken ct)
    {
        if (id != seminar.Id) return BadRequest();
        if (!ModelState.IsValid)
        {
            await PopulateParentOptions(id, ct);
            return View(seminar);
        }

        var result = await seminarService.UpdateAsync(seminar, ct);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            await PopulateParentOptions(id, ct);
            return View(seminar);
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize(Roles = "Admin,Superuser")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var seminar = await seminarService.GetByIdAsync(id, ct);
        if (seminar is null) return NotFound();
        return View(seminar);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Superuser")]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken ct)
    {
        var result = await seminarService.DeleteAsync(id, ct);
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
    public async Task<IActionResult> RecordAttendance(int seminarId, int personId, CancellationToken ct)
    {
        var result = await seminarService.RecordAttendanceAsync(seminarId, personId, ct);
        if (!result.Succeeded) TempData["Error"] = result.Error;
        return RedirectToAction(nameof(Details), new { id = seminarId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Superuser")]
    public async Task<IActionResult> RemoveAttendance(int seminarId, int attendanceId, CancellationToken ct)
    {
        var result = await seminarService.RemoveAttendanceAsync(attendanceId, ct);
        if (!result.Succeeded) TempData["Error"] = result.Error;
        return RedirectToAction(nameof(Details), new { id = seminarId });
    }

    private async Task PopulateParentOptions(int? excludeId, CancellationToken ct)
    {
        var seminars = await seminarService.GetAllAsync(ct);
        ViewBag.ParentOptions = seminars
            .Where(s => s.Id != excludeId)
            .Select(s => new SelectListItem($"{s.City} {s.Year}", s.Id.ToString()))
            .ToList();
    }
}
