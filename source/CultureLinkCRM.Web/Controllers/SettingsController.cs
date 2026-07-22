using CultureLinkCRM.Core.Interfaces;
using CultureLinkCRM.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CultureLinkCRM.Web.Controllers;

// Admin-only (Ref: FR-13, FR-6): the lapsed-donor threshold is a system setting only Admin may change.
[Authorize(Roles = "Admin")]
public class SettingsController(IDonorStatusService donorStatusService) : Controller
{
    public async Task<IActionResult> Index(CancellationToken ct) => View(new SettingsViewModel
    {
        LapsedDonorThresholdMonths = await donorStatusService.GetLapsedThresholdMonthsAsync(ct)
    });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(SettingsViewModel vm, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(vm);

        await donorStatusService.SetLapsedThresholdMonthsAsync(vm.LapsedDonorThresholdMonths, ct);
        TempData["Message"] = "Settings saved.";
        return RedirectToAction(nameof(Index));
    }
}
