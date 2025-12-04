using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FirstWebApplication1.Models;
using FirstWebApplication1.Models.Admin; // Importerer modeller fra prosjektet
using System.Security.Claims;

namespace FirstWebApplication1.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: /Admin/Users
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

        // GET: /Admin/EditUser/{id}
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

        // POST: /Admin/EditUser/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(string id, string? selectedRole, string? organization)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Update Roles
            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!string.IsNullOrEmpty(selectedRole))
            {
                await _userManager.AddToRoleAsync(user, selectedRole);
            }

            // Update Organization Claim
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


            TempData["SuccessMessage"] = "User updated successfully.";
            return RedirectToAction(nameof(Users));
        }

        // POST: /Admin/DeleteUser/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Prevent deleting your own account
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

        // GET: /Admin/Roles
        [HttpGet]
        public async Task<IActionResult> Roles()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            return View(roles);
        }
    }
}