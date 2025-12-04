using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FirstWebApplication1.Models;
using FirstWebApplication1.Models.Admin;
using System.Security.Claims;

namespace FirstWebApplication1.Controllers
{
    /// <summary>
    /// Administrative controller for managing users and roles. Secured by [Authorize] so only Admin role
    /// members can reach these actions.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        /// <summary>
        /// Inject Identity managers for querying and mutating users/roles.
        /// </summary>
        public AdminController(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        /// <summary>
        /// Lists all users with their roles and organization claims.
        /// </summary>
        /// <returns>Users view.</returns>
        [HttpGet]
        public async Task<IActionResult> Users()
        {
            var users = await _userManager.Users.ToListAsync();
            var userViewModels = new List<UsersViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var claims = await _userManager.GetClaimsAsync(user);
                var organizationClaim = claims.FirstOrDefault(c => c.Type == "Organization");

                userViewModels.Add(new UsersViewModel
                {
                    Id = user.Id,
                    Email = user.Email ?? "N/A",
                    Roles = roles.ToList(),
                    Organization = organizationClaim?.Value
                });
            }

            return View(userViewModels);
        }

        /// <summary>
        /// Displays the edit form for a user.
        /// </summary>
        /// <param name="id">User id from the route.</param>
        /// <returns>Edit view or NotFound.</returns>
        [HttpGet]
        public async Task<IActionResult> EditUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            var allRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            var claims = await _userManager.GetClaimsAsync(user);
            var organizationClaim = claims.FirstOrDefault(c => c.Type == "Organization");

            var model = new EditUserViewModel
            {
                Id = user.Id,
                Email = user.Email ?? "N/A",
                CurrentRoles = userRoles.ToList(),
                SelectedRole = userRoles.FirstOrDefault(),
                AvailableRoles = allRoles!,
                Organization = organizationClaim?.Value
            };

            return View(model);
        }

        /// <summary>
        /// Processes the edit form, updating the user's single role and organization claim. Anti-forgery
        /// token defends against CSRF. TempData is used to surface status messages after PRG redirect.
        /// </summary>
        /// <param name="id">User id being edited.</param>
        /// <param name="selectedRole">Role selected in the form.</param>
        /// <param name="organization">Organization claim value.</param>
        /// <returns>Redirects to Users list.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(string id, string? selectedRole, string? organization)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Update Roles (remove existing to enforce single role assignment)
            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!string.IsNullOrEmpty(selectedRole))
            {
                await _userManager.AddToRoleAsync(user, selectedRole);
            }

            // Update Organization Claim (stored as a claim to align with Identity's claims-based auth)
            var claims = await _userManager.GetClaimsAsync(user);
            var organizationClaim = claims.FirstOrDefault(c => c.Type == "Organization");

            if (organizationClaim != null)
            {
                await _userManager.RemoveClaimAsync(user, organizationClaim);
            }
            if (!string.IsNullOrEmpty(organization))
            {
                await _userManager.AddClaimAsync(user, new Claim("Organization", organization));
            }

            TempData["SuccessMessage"] = "User updated successfully."; // PRG message
            return RedirectToAction(nameof(Users));
        }

        /// <summary>
        /// Deletes a user (except the current logged-in admin). Anti-forgery protects against CSRF and
        /// TempData provides user feedback after redirect.
        /// </summary>
        /// <param name="id">Id of the user to delete.</param>
        /// <returns>Redirect to Users list.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Prevent deleting your own account to avoid locking out all admins.
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId == id)
            {
                TempData["ErrorMessage"] = "You cannot delete your own account.";
                return RedirectToAction(nameof(Users));
            }

            var result = await _userManager.DeleteAsync(user);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "User deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to delete user.";
            }

            return RedirectToAction(nameof(Users));
        }

        /// <summary>
        /// Shows all roles defined in the system (currently unused by views but kept for completeness).
        /// </summary>
        /// <returns>Roles view.</returns>
        [HttpGet]
        public async Task<IActionResult> Roles()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            return View(roles);
        }
    }
}
