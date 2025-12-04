using FirstWebApplication1.Models;
using FirstWebApplication1.Models.Account;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace FirstWebApplication1.Controllers
{
    public class AccountController : Controller
    {
        // Identity-tjenester injiseres for håndtering av brukere, innlogging og roller
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly HashSet<string> _allowedAdminEmails;

        public AccountController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;

            // Henter tillatt admin-epost fra config → styrer hvem som kan registrere seg som Admin
            var adminEmail = configuration["Admin:Email"] ?? "admin@kartverket.no";

            // Lagres som HashSet for rask sjekk av lovlige admin-adresser
            _allowedAdminEmails = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                adminEmail
            };
        }

        // Viser registreringssiden
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // Håndterer innsending av registreringsskjema
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Hindrer at hvem som helst registrerer Admin-konto
                if (string.Equals(model.Role, "Admin", StringComparison.OrdinalIgnoreCase) &&
                    !_allowedAdminEmails.Contains(model.Email))
                {
                    ModelState.AddModelError(nameof(model.Role), "Admin registration is restricted to authorized accounts.");
                    return View(model);
                }

                // Oppretter ny Identity-bruker
                var user = new IdentityUser { UserName = model.Email, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Legger bruker i valgt rolle (Pilot / Caseworker / Admin)
                    if (!string.IsNullOrEmpty(model.Role))
                    {
                        await _userManager.AddToRoleAsync(user, model.Role);
                    }

                    // Logger inn brukeren direkte etter registrering
                    await _signInManager.SignInAsync(user, isPersistent: false);

                    // Sender til Home-siden etter vellykket registrering
                    return RedirectToAction("Index", "Home");
                }

                // Viser eventuelle feil fra Identity
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // Returnerer skjemaet på nytt dersom validering feiler
            return View(model);
        }

        // Viser innloggingssiden
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // Håndterer innsending av login-skjema
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                // Forsøker innlogging via Identity
                var result = await _signInManager.PasswordSignInAsync(
                    model.Email,
                    model.Password,
                    model.RememberMe,
                    lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    // Send brukeren tilbake dit de kom fra, ellers Home
                    return RedirectToLocal(returnUrl) ?? RedirectToAction("Index", "Home");
                }

                // Viser standard feilmelding
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }

            return View(model);
        }

        // Logger ut brukeren og sender dem til Home
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        // Vises når brukeren mangler tilgang (autorisering feilet)
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        // Sikrer at redirect bare går til lokale sider → hindrer open redirect-angrep
        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
    }
}
