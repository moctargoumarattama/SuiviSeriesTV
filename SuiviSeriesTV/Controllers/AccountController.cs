using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SuiviSeriesTV.Constants;
using SuiviSeriesTV.Models;
using SuiviSeriesTV.Services;
using SuiviSeriesTV.ViewModels.Account;

namespace SuiviSeriesTV.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IEmailService _emailService;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IEmailService emailService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _emailService = emailService;
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        return View(new RegisterViewModel());
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        await _userManager.AddToRoleAsync(user, AppRoles.User);

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var callbackUrl = Url.Action(
            nameof(ConfirmEmail),
            "Account",
            new { userId = user.Id, token },
            protocol: Request.Scheme);

        var body = $"""
                    <p>Bonjour,</p>
                    <p>Merci pour votre inscription sur <strong>SuiviSeriesTV</strong>.</p>
                    <p>Confirmez votre email en cliquant ici:</p>
                    <p><a href="{callbackUrl}">Confirmer mon compte</a></p>
                    """;

        await _emailService.SendEmailAsync(user.Email!, "Confirmez votre compte SuiviSeriesTV", body);

        return RedirectToAction(nameof(CheckEmail), new { email = user.Email });
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult CheckEmail(string? email)
    {
        ViewData["Email"] = email;
        return View();
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> ConfirmEmail(string? userId, string? token)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
        {
            ViewData["Status"] = "Lien invalide.";
            ViewData["Success"] = false;
            return View();
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            ViewData["Status"] = "Utilisateur introuvable.";
            ViewData["Success"] = false;
            return View();
        }

        var result = await _userManager.ConfirmEmailAsync(user, token);
        ViewData["Success"] = result.Succeeded;
        ViewData["Status"] = result.Succeeded
            ? "Votre email a bien ete confirme. Vous pouvez maintenant vous connecter."
            : "Echec de confirmation. Le lien a peut-etre expire.";

        return View();
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user is null)
        {
            ModelState.AddModelError(string.Empty, "Email ou mot de passe invalide.");
            return View(model);
        }

        if (!user.EmailConfirmed)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var callbackUrl = Url.Action(
                nameof(ConfirmEmail),
                "Account",
                new { userId = user.Id, token },
                protocol: Request.Scheme);

            var body = $"""
                        <p>Votre email n'est pas encore confirme.</p>
                        <p>Cliquez ici pour confirmer votre compte:</p>
                        <p><a href="{callbackUrl}">Confirmer mon email</a></p>
                        """;

            await _emailService.SendEmailAsync(user.Email!, "Confirmation email SuiviSeriesTV", body);

            ModelState.AddModelError(string.Empty, "Email non confirme. Un nouveau lien vient d'etre envoye.");
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(
            user,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: true);

        if (result.Succeeded)
        {
            if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            if (await _userManager.IsInRoleAsync(user, AppRoles.Admin))
            {
                return RedirectToAction("Index", "Admin");
            }

            return RedirectToAction("Index", "User");
        }

        if (result.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty, "Votre compte est temporairement bloque. Reessayez plus tard.");
            return View(model);
        }

        ModelState.AddModelError(string.Empty, "Email ou mot de passe invalide.");
        return View(model);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }
}
