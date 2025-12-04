using FirstWebApplication1.Models;
using FirstWebApplication1.Models.Account;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;

namespace FirstWebApplication1.Controllers
{
    /// <summary>
    /// Handles authentication and authorization flows (register, login, logout). Uses ASP.NET Core Identity
    /// to issue secure cookies (XSS-protected, HttpOnly) and relies on antiforgery tokens on POST actions.
    /// </summary>
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly HashSet<string> _allowedAdminEmails;

        /// <summary>
        /// Constructs the controller with Identity managers injected by DI.
        /// </summary>
        /// <param name="userManager">Manages user creation and claims.</param>
        /// <param name="signInManager">Issues/validates auth cookies.</param>
        /// <param name="roleManager">Resolves role information.</param>
        /// <param name="configuration">Used to restrict admin creation via configured email.</param>
        public AccountController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;

            var adminEmail = configuration["Admin:Email"] ?? "admin@kartverket.no";
            _allowedAdminEmails = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                adminEmail
            };
        }

        /// <summary>
        /// Displays the registration form.
        /// </summary>
        /// <returns>Register view.</returns>
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        /// <summary>
        /// Handles registration form submission. Uses ModelState to re-render the form on validation errors
        /// (PRG pattern), enforces admin email restrictions, and uses Identity to hash/store credentials.
        /// </summary>
        /// <param name="model">Posted registration data.</param>
        /// <returns>Redirect to Home on success; view with errors on failure.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken] // Anti-forgery token defends against CSRF on form submission.
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (string.Equals(model.Role, "Admin", StringComparison.OrdinalIgnoreCase) &&
                    !_allowedAdminEmails.Contains(model.Email))
                {
                    // Prevent arbitrary users from self-promoting to admin.
                    ModelState.AddModelError(nameof(model.Role), "Admin registration is restricted to authorized accounts.");
                    return View(model);
                }

                var user = new IdentityUser { UserName = model.Email, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Assign role to user with parameterized DB calls through Identity/EF (SQL injection safe).
                    if (!string.IsNullOrEmpty(model.Role))
                    {
                        await _userManager.AddToRoleAsync(user, model.Role);
                    }

                    await _signInManager.SignInAsync(user, isPersistent: false); // Issues auth cookie.
                    return RedirectToAction("Index", "Home"); // Post/Redirect/Get to avoid form re-posts.
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        /// <summary>
        /// Displays the login form. Accepts optional returnUrl for redirect after successful login.
        /// </summary>
        /// <param name="returnUrl">Local URL to redirect to after login.</param>
        /// <returns>Login view.</returns>
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl; // Stored in ViewData to maintain PRG flow.
            return View();
        }

        /// <summary>
        /// Authenticates the user via Identity. ModelState ensures credentials are present; Identity protects
        /// against timing attacks and locks per configuration. Uses ValidateAntiForgeryToken for CSRF.
        /// </summary>
        /// <param name="model">Login credentials.</param>
        /// <param name="returnUrl">Optional redirect target validated via Url.IsLocalUrl.</param>
        /// <returns>Redirect to returnUrl/home on success; re-renders login on failure.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    return RedirectToLocal(returnUrl) ?? RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }

            return View(model);
        }

        /// <summary>
        /// Signs the current user out by clearing the auth cookie.
        /// </summary>
        /// <returns>Redirect to home.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// Rendered when a user lacks sufficient authorization to access a resource.
        /// </summary>
        /// <returns>Access denied view.</returns>
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        /// <summary>
        /// Validates that the returnUrl is local to prevent open redirects. Used after successful login.
        /// </summary>
        /// <param name="returnUrl">Target URL supplied by query string.</param>
        /// <returns>Redirect result when safe; otherwise redirects to Home/Index.</returns>
        private IActionResult? RedirectToLocal(string? returnUrl)
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
