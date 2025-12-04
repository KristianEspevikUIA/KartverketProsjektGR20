using FirstWebApplication1.Data;
using FirstWebApplication1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;


namespace FirstWebApplication1.Controllers
{
    [EnableRateLimiting("Fixed")] // Rate limiting for alle requests til denne controlleren
    public class ObstacleController : Controller
    {
        private readonly ApplicationDbContext _context; // DB-kontekst for hindere
        private readonly UserManager<IdentityUser> _userManager; // Bruker/rollehåndtering

        public ObstacleController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context; // DI setter databasekonteksten
            _userManager = userManager; // DI setter brukertjenesten
        }

        // Hjelpemetode: Sjekker om brukeren er pilot
        private bool IsPilot()
        {
            return User.IsInRole("Pilot");
        }

        // STEP 1: Velge hindertype
        [Authorize]
        [HttpGet]
        public IActionResult SelectType()
        {
            return View(new ObstacleTypeViewModel()); // Sender tom modell til view
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SelectType(ObstacleTypeViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.SelectedType)) // Må velges
            {
                ModelState.AddModelError("SelectedType", "Please select an obstacle type");
                return View(model);
            }

            TempData["ObstacleType"] = model.SelectedType; // Lagrer valg midlertidig
            return RedirectToAction(nameof(DataForm)); // Går videre til steg 2
        }

        // STEP 2: Fyll inn detaljer
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> DataForm()
        {
            if (TempData.Peek("ObstacleType") == null) // Hindrer å hoppe over steg
            {
                return RedirectToAction(nameof(SelectType));
            }

            var obstacleType = TempData.Peek("ObstacleType")?.ToString();

            // Oppretter startmodell med defaulthøyde
            var obstacleData = new ObstacleData
            {
                ObstacleType = obstacleType,
                ObstacleHeight = 15
            };

            // Henter godkjente hindere for å vise på kartet
            var approvedObstacles = await _context.Obstacles
                .Where(o => o.Status == "Approved")
                .AsNoTracking()
                .ToListAsync();

            // Holder PascalCase i JSONet
            var serializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = null
            };

            // Serialiserer enklere datasett til kartet
            var approvedObstaclesJson = JsonSerializer.Serialize(approvedObstacles.Select(o => new
            {
                o.ObstacleName,
                o.ObstacleType,
                o.Latitude,
                o.Longitude,
                o.LineGeoJson
            }), serializerOptions);

            ViewBag.ApprovedObstaclesJson = approvedObstaclesJson; // Sender JSON til view

            // Sender info om brukerens rolle
            ViewBag.IsPilot = IsPilot();
            ViewBag.UsesFeet = IsPilot();

            return View(obstacleData);
        }

        // STEP 3: Lagre og vis oversikt
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DataForm(ObstacleData obstacledata, bool? useFeet)
        {
            try
            {
                // Henter hinder-type fra TempData
                var obstacleType = TempData.Peek("ObstacleType")?.ToString();
                if (!string.IsNullOrWhiteSpace(obstacleType))
                {
                    obstacledata.ObstacleType = obstacleType;
                }

                // Automatisk navn dersom ikke annet oppgis
                if (!string.IsNullOrWhiteSpace(obstacledata.ObstacleType))
                {
                    obstacledata.ObstacleName = obstacledata.ObstacleType;
                }
                else
                {
                    obstacledata.ObstacleName = "Unknown Obstacle";
                }

                // Hindrer null-beskrivelse
                if (string.IsNullOrWhiteSpace(obstacledata.ObstacleDescription))
                {
                    obstacledata.ObstacleDescription = "";
                }

                // Fjerner validering på auto-genererte felt
                ModelState.Remove("ObstacleType");
                ModelState.Remove("ObstacleName");
                ModelState.Remove("ObstacleDescription");
                ModelState.Remove("Organization");

                var isPilot = IsPilot();
                var usesFeetPreference = useFeet ?? isPilot;

                if (!ModelState.IsValid) // Ved valideringsfeil: returner view
                {
                    ViewBag.IsPilot = isPilot;
                    ViewBag.UsesFeet = usesFeetPreference;
                    return View(obstacledata);
                }

                // Henter organisasjon fra claims
                var organizationClaim = User.Claims.FirstOrDefault(c => c.Type == "Organization");

                // Metadata ved innsending
                obstacledata.Status = "Pending";
                obstacledata.SubmittedBy = User.Identity?.Name ?? "Unknown";
                obstacledata.Organization = organizationClaim?.Value;
                obstacledata.SubmittedDate = DateTime.UtcNow;

                _context.Obstacles.Add(obstacledata); // Legger til i databasen
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Overview), new { id = obstacledata.Id, useFeet = usesFeetPreference });
            }
            catch (Exception ex) // Feilhåndtering
            {
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                ModelState.AddModelError("", $"Error saving obstacle: {innerMessage}");

                var isPilot = IsPilot();
                ViewBag.IsPilot = isPilot;
                ViewBag.UsesFeet = useFeet ?? isPilot;

                return View(obstacledata);
            }
        }

        // STEP 4: Oversikt etter innsending
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Overview(int id, bool? useFeet)
        {
            // Henter hinderet by ID
            var obstacle = await _context.Obstacles
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == id);

            if (obstacle is null)
            {
                return NotFound();
            }

            var isPilot = IsPilot();
            var usesFeetPreference = useFeet ?? isPilot;

            ViewBag.IsPilot = isPilot;
            ViewBag.UsesFeet = usesFeetPreference;

            return View(obstacle);
        }

        // LIST, FILTERING, SEARCHING
        [Authorize(Roles = "Pilot,Caseworker,Admin")]
        [HttpGet]
        public async Task<IActionResult> List(string? statusFilter = null, string? searchTerm = null, DateTime? startDate = null, DateTime? endDate = null, string? obstacleTypeFilter = null, string? organizationFilter = null, double? minHeight = null, double? maxHeight = null)
        {
            var obstaclesQuery = _context.Obstacles.AsQueryable(); // Start query

            // Filter etter status
            if (!string.IsNullOrWhiteSpace(statusFilter))
            {
                var normalizedFilter = statusFilter.Trim();
                if (string.Equals(normalizedFilter, "Rejected", StringComparison.OrdinalIgnoreCase))
                {
                    normalizedFilter = "Declined"; // Normaliserer "rejected" → "declined"
                }
                obstaclesQuery = obstaclesQuery.Where(o => o.Status == normalizedFilter);
            }

            // Filter etter organisasjon
            if (!string.IsNullOrWhiteSpace(organizationFilter))
            {
                obstaclesQuery = obstaclesQuery.Where(o => o.Organization == organizationFilter);
            }

            // Filter etter hinder-type
            if (!string.IsNullOrWhiteSpace(obstacleTypeFilter))
            {
                obstaclesQuery = obstaclesQuery.Where(o => o.ObstacleType == obstacleTypeFilter);
            }

            // Tekstsøk i navn
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                obstaclesQuery = obstaclesQuery.Where(o => o.ObstacleName != null && o.ObstacleName.Contains(searchTerm));
            }

            // Minimum høyde
            if (minHeight.HasValue)
            {
                obstaclesQuery = obstaclesQuery.Where(o => o.ObstacleHeight >= minHeight.Value);
            }

            // Maksimum høyde
            if (maxHeight.HasValue)
            {
                obstaclesQuery = obstaclesQuery.Where(o => o.ObstacleHeight <= maxHeight.Value);
            }

            // Dato fra …
            if (startDate.HasValue)
            {
                obstaclesQuery = obstaclesQuery.Where(o => o.SubmittedDate >= startDate.Value);
            }

            // … og til (inkluderer hele siste dag)
            if (endDate.HasValue)
            {
                obstaclesQuery = obstaclesQuery.Where(o => o.SubmittedDate < endDate.Value.AddDays(1));
            }

            // Kjører query og sorterer
            var obstacles = await obstaclesQuery
                .OrderByDescending(o => o.SubmittedDate)
                .ToListAsync();

            var viewModel = new ObstacleListViewModel // Lager visningsmodell
            {
                Obstacles = obstacles,
                StatusFilter = statusFilter,
                SearchTerm = searchTerm,
                StartDate = startDate,
                EndDate = endDate,
                ObstacleTypeFilter = obstacleTypeFilter,
                OrganizationFilter = organizationFilter,
                MinHeight = minHeight,
                MaxHeight = maxHeight
            };

            ViewBag.IsPilot = IsPilot();
            ViewBag.UsesFeet = IsPilot();

            return View(viewModel);
        }

        // VIS DETALJER
        [Authorize(Roles = "Pilot,Caseworker,Admin")]
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var obstacle = await _context.Obstacles.FindAsync(id); // Henter hinderet

            if (obstacle == null)
            {
                return NotFound();
            }

            ViewBag.IsPilot = IsPilot();
            ViewBag.UsesFeet = IsPilot();

            return View(obstacle);
        }

        // REDIGER HINDER (GET)
        [Authorize(Roles = "Pilot,Caseworker,Admin")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var obstacle = await _context.Obstacles.FindAsync(id); // Henter hinderet

            if (obstacle == null)
            {
                return NotFound();
            }

            // Sikkerhet: Piloter kan kun redigere egne innsendelser
            if (User.IsInRole("Pilot") && obstacle.SubmittedBy != User.Identity.Name)
            {
                return Forbid();
            }

            ViewBag.IsPilot = IsPilot();
            ViewBag.UsesFeet = IsPilot();

            return View(obstacle);
        }

        // REDIGER HINDER (POST)
        [Authorize(Roles = "Pilot,Caseworker,Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id, ObstacleName, ObstacleHeight, ObstacleDescription, Longitude, Latitude, LineGeoJson")] ObstacleData obstacledata)
        {
            if (id != obstacledata.Id) // URL-ID må matche skjema-ID
            {
                return NotFound();
            }

            var obstacleToUpdate = await _context.Obstacles.FindAsync(id);

            if (obstacleToUpdate == null)
            {
                return NotFound();
            }

            // Sikkerhet: piloter kan kun redigere egne hindere
            if (User.IsInRole("Pilot") && obstacleToUpdate.SubmittedBy != User.Identity.Name)
            {
                return Forbid();
            }

            ModelState.Remove("Organization"); // Fjerner validering av dette feltet

            if (!ModelState.IsValid)
            {
                ViewBag.IsPilot = IsPilot();
                ViewBag.UsesFeet = IsPilot();

                // Returnerer original databaseentitet for å unngå tap av felt
                return View(obstacleToUpdate);
            }

            try
            {
                // Oppdaterer felter med data brukeren har endret
                obstacleToUpdate.ObstacleName = obstacledata.ObstacleName;
                obstacleToUpdate.ObstacleHeight = obstacledata.ObstacleHeight;
                obstacleToUpdate.ObstacleDescription = obstacledata.ObstacleDescription ?? "";
                obstacleToUpdate.Longitude = obstacledata.Longitude;
                obstacleToUpdate.Latitude = obstacledata.Latitude;
                obstacleToUpdate.LineGeoJson = obstacledata.LineGeoJson;

                // Setter metadata for endring
                obstacleToUpdate.LastModifiedBy = User.Identity?.Name ?? "Unknown";
                obstacleToUpdate.LastModifiedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await ObstacleExists(id)) // Sjekker at hinderet fortsatt finnes
                {
                    return NotFound();
                }
                throw;
            }

            return RedirectToAction(nameof(List)); // Tilbake til listevisningen
        }

        // GODKJENN HINDER
        [Authorize(Roles = "Caseworker,Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var obstacle = await _context.Obstacles.FindAsync(id);

            if (obstacle == null)
            {
                return NotFound();
            }

            // Oppdaterer status og metadata
            obstacle.Status = "Approved";
            obstacle.ApprovedBy = User.Identity?.Name ?? "Unknown";
            obstacle.ApprovedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["NotificationMessage"] = "Obstacle was approved";
            TempData["NotificationType"] = "success";

            return RedirectToAction(nameof(List), new { statusFilter = "Pending" });
        }

        // AVSLÅ HINDER
        [Authorize(Roles = "Caseworker,Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Decline(int id, string? declineReason)
        {
            var obstacle = await _context.Obstacles.FindAsync(id);

            if (obstacle == null)
            {
                return NotFound();
            }

            obstacle.Status = "Declined";
            obstacle.DeclinedBy = User.Identity?.Name ?? "Unknown";
            obstacle.DeclinedDate = DateTime.UtcNow;
            obstacle.DeclineReason = declineReason;

            await _context.SaveChangesAsync();

            TempData["NotificationMessage"] = "Obstacle was rejected";
            TempData["NotificationType"] = "error";

            return RedirectToAction(nameof(List), new { statusFilter = "Pending" });
        }

        // GJØR HINDER TILBAKE TIL PENDING
        [Authorize(Roles = "Caseworker,Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Revalidate(int id)
        {
            var obstacle = await _context.Obstacles.FindAsync(id);

            if (obstacle == null)
            {
                return NotFound();
            }

            obstacle.Status = "Pending";

            obstacle.LastModifiedBy = User.Identity?.Name ?? "Unknown";
            obstacle.LastModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["NotificationMessage"] = "Obstacle was set back to Pending for re-evaluation";
            TempData["NotificationType"] = "warning";

            return RedirectToAction(nameof(Details), new { id });
        }

        // SLETT HINDER (kun Admin)
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var obstacle = await _context.Obstacles.FindAsync(id);

            if (obstacle == null)
            {
                return NotFound();
            }

            _context.Obstacles.Remove(obstacle); // Fjerner entitet
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(List));
        }

        // Hjelpemetode: sjekker om hinder eksisterer
        private async Task<bool> ObstacleExists(int id)
        {
            return await _context.Obstacles.AnyAsync(e => e.Id == id);
        }
    }
}
