using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FirstWebApplication1.Models;
using FirstWebApplication1.Models.Admin; // Importerer modeller fra prosjektet
using System.Security.Claims;

namespace FirstWebApplication1.Controllers
{
    [Authorize(Roles = "Admin")] // Kun Admin-rollen har tilgang til denne controlleren
    public class AdminController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager; // Håndterer CRUD for brukere
        private readonly RoleManager<IdentityRole> _roleManager; // Håndterer roller i systemet

        public AdminController(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager; // DI setter brukerhåndtering
            _roleManager = roleManager; // DI setter rolleadministrasjon
        }

        // Viser en oversikt over alle brukere i systemet
        [HttpGet]
        public async Task<IActionResult> Users()
        {
            var users = await _userManager.Users.ToListAsync(); // Henter alle brukere fra databasen
            var userViewModels = new List<UsersViewModel>(); // Liste for visningsmodeller

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user); // Henter brukerens roller
                var claims = await _userManager.GetClaimsAsync(user); // Henter claimene til brukeren
                var organizationClaim = claims.FirstOrDefault(c => c.Type == "Organization"); // Sjekker om brukeren har organisasjons-claim

                userViewModels.Add(new UsersViewModel
                {
                    Id = user.Id,
                    Email = user.Email ?? "N/A",
                    Roles = roles.ToList(), // Sender med roller til view
                    Organization = organizationClaim?.Value // Organisasjon hvis den finnes
                });
            }

            return View(userViewModels); // Viser oversiktssiden med alle brukere
        }

        // Viser siden for å redigere en enkelt bruker
        [HttpGet]
        public async Task<IActionResult> EditUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id); // Henter bruker basert på ID
            if (user == null)
            {
                return NotFound(); // Returnerer 404 hvis brukeren ikke finnes
            }

            var userRoles = await _userManager.GetRolesAsync(user); // Henter brukerens eksisterende roller
            var allRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync(); // Alle roller i systemet
            var claims = await _userManager.GetClaimsAsync(user); // Henter claims til bruker
            var organizationClaim = claims.FirstOrDefault(c => c.Type == "Organization"); // Henter organisasjon

            var model = new EditUserViewModel
            {
                Id = user.Id,
                Email = user.Email ?? "N/A",
                CurrentRoles = userRoles.ToList(), // Alle roller brukeren har
                SelectedRole = userRoles.FirstOrDefault(), // Velger første rolle som default
                AvailableRoles = allRoles!, // Viser alle roller i dropdown
                Organization = organizationClaim?.Value // Organisasjons-claim verdi
            };

            return View(model); // Viser redigeringsskjemaet
        }

        // Utfører oppdatering av en bruker etter innsending av skjema
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(string id, string? selectedRole, string? organization)
        {
            var user = await _userManager.FindByIdAsync(id); // Henter bruker etter ID
            if (user == null)
            {
                return NotFound();
            }

            // Oppdaterer roller ved å fjerne gamle og legge til valgt rolle
            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles); // Fjerner alle eksisterende roller

            if (!string.IsNullOrEmpty(selectedRole))
            {
                await _userManager.AddToRoleAsync(user, selectedRole); // Legger til ny rolle
            }

            // Oppdaterer organisasjons-claim
            var claims = await _userManager.GetClaimsAsync(user);
            var organizationClaim = claims.FirstOrDefault(c => c.Type == "Organization");

            if (organizationClaim != null)
            {
                await _userManager.RemoveClaimAsync(user, organizationClaim); // Fjerner gammel claim
            }
            if (!string.IsNullOrEmpty(organization))
            {
                await _userManager.AddClaimAsync(user, new Claim("Organization", organization)); // Legger til ny claim
            }

            TempData["SuccessMessage"] = "User updated successfully."; // Tilbakemelding til admin
            return RedirectToAction(nameof(Users)); // Tilbake til brukerlisten
        }

        // Sletter en bruker
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id); // Sjekker om bruker eksisterer
            if (user == null)
            {
                return NotFound();
            }

            // Hindrer at admin sletter sin egen konto
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId == id)
            {
                TempData["ErrorMessage"] = "You cannot delete your own account."; // Sikkerhetsmekanisme
                return RedirectToAction(nameof(Users));
            }

            var result = await _userManager.DeleteAsync(user); // Utfører sletting

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "User deleted successfully."; // OK-melding
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to delete user."; // Feil-melding
            }

            return RedirectToAction(nameof(Users)); // Tilbake til brukeroversikt
        }

        // Viser en liste over alle roller i systemet
        [HttpGet]
        public async Task<IActionResult> Roles()
        {
            var roles = await _roleManager.Roles.ToListAsync(); // Henter alle roller
            return View(roles); // Viser rollelisten
        }
    }
}
