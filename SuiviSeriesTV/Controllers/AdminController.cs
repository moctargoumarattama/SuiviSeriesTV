using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SuiviSeriesTV.Constants;
using SuiviSeriesTV.Data;
using SuiviSeriesTV.Models;
using SuiviSeriesTV.ViewModels.Admin;

namespace SuiviSeriesTV.Controllers;

[Authorize(Roles = AppRoles.Admin)]
public class AdminController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;

    public AdminController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var users = await _userManager.Users.ToListAsync();

        var adminCount = 0;
        var lockedCount = 0;
        foreach (var user in users)
        {
            if (await _userManager.IsInRoleAsync(user, AppRoles.Admin))
            {
                adminCount++;
            }

            if (IsLocked(user))
            {
                lockedCount++;
            }
        }

        var viewModel = new AdminDashboardViewModel
        {
            TotalUsers = users.Count,
            TotalAdmins = adminCount,
            ConfirmedEmails = users.Count(u => u.EmailConfirmed),
            LockedUsers = lockedCount
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Users(string? search)
    {
        var query = _userManager.Users.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(u =>
                (u.Email != null && u.Email.Contains(search)) ||
                (u.UserName != null && u.UserName.Contains(search)));
        }

        var users = await query.OrderBy(u => u.Email).ToListAsync();
        var items = new List<AdminUserItemViewModel>(users.Count);
        foreach (var user in users)
        {
            items.Add(new AdminUserItemViewModel
            {
                Id = user.Id,
                Email = user.Email ?? "(sans email)",
                UserName = user.UserName ?? "(sans nom)",
                EmailConfirmed = user.EmailConfirmed,
                IsAdmin = await _userManager.IsInRoleAsync(user, AppRoles.Admin),
                IsLocked = IsLocked(user),
                CreatedAtUtc = user.CreatedAtUtc
            });
        }

        ViewData["Search"] = search;
        return View(items);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleLock(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
        {
            TempData["ErrorMessage"] = "Utilisateur introuvable.";
            return RedirectToAction(nameof(Users));
        }

        if (IsLocked(user))
        {
            user.LockoutEnd = null;
            TempData["SuccessMessage"] = $"Le compte {user.Email} a ete debloque.";
        }
        else
        {
            user.LockoutEnd = DateTimeOffset.UtcNow.AddYears(50);
            TempData["SuccessMessage"] = $"Le compte {user.Email} a ete bloque.";
        }

        await _userManager.UpdateAsync(user);
        return RedirectToAction(nameof(Users));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleAdminRole(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
        {
            TempData["ErrorMessage"] = "Utilisateur introuvable.";
            return RedirectToAction(nameof(Users));
        }

        var currentUserId = _userManager.GetUserId(User);
        if (string.Equals(currentUserId, user.Id, StringComparison.Ordinal))
        {
            TempData["ErrorMessage"] = "Vous ne pouvez pas modifier votre propre role admin ici.";
            return RedirectToAction(nameof(Users));
        }

        if (await _userManager.IsInRoleAsync(user, AppRoles.Admin))
        {
            await _userManager.RemoveFromRoleAsync(user, AppRoles.Admin);
            await _userManager.AddToRoleAsync(user, AppRoles.User);
            TempData["SuccessMessage"] = $"{user.Email} est maintenant utilisateur standard.";
        }
        else
        {
            await _userManager.AddToRoleAsync(user, AppRoles.Admin);
            TempData["SuccessMessage"] = $"{user.Email} est maintenant administrateur.";
        }

        return RedirectToAction(nameof(Users));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RenameUser(string id, string newUserName)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
        {
            TempData["ErrorMessage"] = "Utilisateur introuvable.";
            return RedirectToAction(nameof(Users));
        }

        var trimmed = (newUserName ?? string.Empty).Trim();
        if (trimmed.Length < 3 || trimmed.Length > 32)
        {
            TempData["ErrorMessage"] = "Le nom utilisateur doit contenir entre 3 et 32 caracteres.";
            return RedirectToAction(nameof(Users));
        }

        var result = await _userManager.SetUserNameAsync(user, trimmed);
        if (!result.Succeeded)
        {
            TempData["ErrorMessage"] = "Renommage impossible: " + string.Join("; ", result.Errors.Select(e => e.Description));
            return RedirectToAction(nameof(Users));
        }

        TempData["SuccessMessage"] = $"Nom utilisateur mis a jour pour {user.Email}.";
        return RedirectToAction(nameof(Users));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
        {
            TempData["ErrorMessage"] = "Utilisateur introuvable.";
            return RedirectToAction(nameof(Users));
        }

        var currentUserId = _userManager.GetUserId(User);
        if (string.Equals(currentUserId, user.Id, StringComparison.Ordinal))
        {
            TempData["ErrorMessage"] = "Vous ne pouvez pas supprimer votre propre compte.";
            return RedirectToAction(nameof(Users));
        }

        var userSeries = _context.Series.Where(s => s.OwnerId == user.Id);
        _context.Series.RemoveRange(userSeries);
        await _context.SaveChangesAsync();

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            TempData["ErrorMessage"] = "Suppression impossible: " + string.Join("; ", result.Errors.Select(e => e.Description));
            return RedirectToAction(nameof(Users));
        }

        TempData["SuccessMessage"] = $"Le compte {user.Email} a ete supprime.";
        return RedirectToAction(nameof(Users));
    }

    private static bool IsLocked(ApplicationUser user)
    {
        return user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow;
    }
}
