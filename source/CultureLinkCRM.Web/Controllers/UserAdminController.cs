using CultureLinkCRM.Core.Interfaces;
using CultureLinkCRM.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CultureLinkCRM.Web.Controllers;

// Admin-only (Ref: FR-13): User/Role management is restricted to the Admin role and denied for everyone else.
[Authorize(Roles = "Admin")]
public class UserAdminController(IUserService userService) : Controller
{
    public async Task<IActionResult> Index(CancellationToken ct) => View(await userService.GetAllAsync(ct));

    public IActionResult Create() => View(new CreateUserViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateUserViewModel vm, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(vm);

        var result = await userService.CreateAsync(vm.Email, vm.Password, vm.Role, ct);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            return View(vm);
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var user = await userService.GetByIdAsync(id, ct);
        if (user is null) return NotFound();
        return View(new UpdateUserRoleViewModel { Id = user.Id, Email = user.Email, Role = user.Role });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdateUserRoleViewModel vm, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(vm);

        var result = await userService.UpdateRoleAsync(vm.Id, vm.Role, ct);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            return View(vm);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await userService.DeleteAsync(id, ct);
        if (!result.Succeeded)
        {
            TempData["Error"] = result.Error;
        }
        return RedirectToAction(nameof(Index));
    }
}
