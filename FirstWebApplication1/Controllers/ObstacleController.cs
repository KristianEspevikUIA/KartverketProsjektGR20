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
    /// <summary>
    /// Core controller that drives the obstacle registration workflow (SelectType -> DataForm -> Overview)
    /// and the CRUD/review pipeline. Uses [Authorize] to protect sensitive actions and enforces that pilots
    /// may only edit their own obstacles. Rate limiting is enabled at the controller level.
    /// </summary>
    [EnableRateLimiting("Fixed")]
    public class ObstacleController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        /// <summary>
        /// Injects EF Core DbContext and Identity user manager for ownership checks.
        /// </summary>
        public ObstacleController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /// <summary>
        /// Helper to check if the current user belongs to the Pilot role. Used to drive UI choices and
        /// authorization logic (e.g., editing only own obstacles).
        /// </summary>
        private bool IsPilot()
        {
            return User.IsInRole("Pilot");
        }

        /// <summary>
        /// Step 1: renders obstacle type selection view.
        /// </summary>
        [Authorize]
        [HttpGet]
        public IActionResult SelectType()
        {
            return View(new ObstacleTypeViewModel());
        }

        /// <summary>
        /// Handles the obstacle type selection submission. TempData is used to carry the chosen type to the
        /// next step (DataForm) following the PRG pattern. Anti-forgery token blocks CSRF.
        /// </summary>
        /// <param name="model">Selected obstacle type.</param>
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SelectType(ObstacleTypeViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.SelectedType))
            {
                ModelState.AddModelError("SelectedType", "Please select an obstacle type");
                return View(model);
            }

            TempData["ObstacleType"] = model.SelectedType; // Stored temporarily to survive redirect.
            return RedirectToAction(nameof(DataForm));
        }

        /// <summary>
        /// Step 2: displays the obstacle data entry form pre-populated with the selected type. Includes a
        /// JSON list of approved obstacles for map visualization. AsNoTracking used for read-only queries.
        /// </summary>
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> DataForm()
        {
            if (TempData.Peek("ObstacleType") == null)
            {
                return RedirectToAction(nameof(SelectType));
            }

            var obstacleType = TempData.Peek("ObstacleType")?.ToString();
            var obstacleData = new ObstacleData
            {
                ObstacleType = obstacleType,
                ObstacleHeight = 15 // Default minimum height in meters
            };

            var approvedObstacles = await _context.Obstacles
                .Where(o => o.Status == "Approved")
                .AsNoTracking() // Avoids change tracking for read-only data and improves performance.
                .ToListAsync();

            // Create serializer options to keep property names as-is (PascalCase)
            var serializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = null
            };

            var approvedObstaclesJson = JsonSerializer.Serialize(approvedObstacles.Select(o => new
            {
                o.ObstacleName,
                o.ObstacleType,
                o.Latitude,
                o.Longitude,
                o.LineGeoJson
            }), serializerOptions); // Apply the options here

            ViewBag.ApprovedObstaclesJson = approvedObstaclesJson;

            // Pass user role info to view so UI can toggle feet/meters and available actions.
            ViewBag.IsPilot = IsPilot();
            ViewBag.UsesFeet = IsPilot();

            return View(obstacleData);
        }

        /// <summary>
        /// Step 3: processes obstacle submission. Enforces server-side validation, captures ownership
        /// (SubmittedBy) so pilots can edit only their own entries, and uses PRG to redirect to Overview.
        /// Anti-forgery token mitigates CSRF; EF Core parameterizes SQL for safety.
        /// </summary>
        /// <param name="obstacledata">Posted obstacle data.</param>
        /// <param name="useFeet">Optional toggle for UI unit preference.</param>
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DataForm(ObstacleData obstacledata, bool? useFeet)
        {
            try
            {
                // Retrieve obstacle type from TempData
                var obstacleType = TempData.Peek("ObstacleType")?.ToString();
                if (!string.IsNullOrWhiteSpace(obstacleType))
                {
                    obstacledata.ObstacleType = obstacleType;
                }

                // Auto-generate obstacle name from type
                if (!string.IsNullOrWhiteSpace(obstacledata.ObstacleType))
                {
                    obstacledata.ObstacleName = obstacledata.ObstacleType;
                }
                else
                {
                    obstacledata.ObstacleName = "Unknown Obstacle";
                }

                // FIX: Ensure description is not null
                if (string.IsNullOrWhiteSpace(obstacledata.ObstacleDescription))
                {
                    obstacledata.ObstacleDescription = "";
                }

                // Remove validation for optional/auto-generated fields to avoid ModelState errors.
                ModelState.Remove("ObstacleType");
                ModelState.Remove("ObstacleName");
                ModelState.Remove("ObstacleDescription");
                ModelState.Remove("Organization");

                var isPilot = IsPilot();
                var usesFeetPreference = useFeet ?? isPilot;

                if (!ModelState.IsValid)
                {
                    ViewBag.IsPilot = isPilot;
                    ViewBag.UsesFeet = usesFeetPreference;
                    return View(obstacledata);
                }

                // Get organization from user claims for auditing/filtering.
                var organizationClaim = User.Claims.FirstOrDefault(c => c.Type == "Organization");

                // Set status to Pending and capture submitter metadata.
                obstacledata.Status = "Pending";
                obstacledata.SubmittedBy = User.Identity?.Name ?? "Unknown";
                obstacledata.Organization = organizationClaim?.Value;
                obstacledata.SubmittedDate = DateTime.UtcNow;

                _context.Obstacles.Add(obstacledata);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Overview), new { id = obstacledata.Id, useFeet = usesFeetPreference });
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                ModelState.AddModelError("", $"Error saving obstacle: {innerMessage}");

                var isPilot = IsPilot();
                ViewBag.IsPilot = isPilot;
                ViewBag.UsesFeet = useFeet ?? isPilot;

                return View(obstacledata);
            }
        }

        /// <summary>
        /// Step 4: shows a read-only overview of the submitted obstacle. AsNoTracking prevents accidental edits
        /// and improves performance. Uses the useFeet flag to render appropriate units.
        /// </summary>
        /// <param name="id">Obstacle id from route.</param>
        /// <param name="useFeet">Optional unit preference from previous step.</param>
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Overview(int id, bool? useFeet)
        {
            // Retrieve the obstacle by ID from the database
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

        /// <summary>
        /// Lists obstacles with optional filters. Authorization allows pilots/caseworkers/admins. Uses
        /// IQueryable to build parameterized SQL server-side and avoid loading unnecessary rows.
        /// </summary>
        [Authorize(Roles = "Pilot,Caseworker,Admin")]
        [HttpGet]
        public async Task<IActionResult> List(string? statusFilter = null, string? searchTerm = null, DateTime? startDate = null, DateTime? endDate = null, string? obstacleTypeFilter = null, string? organizationFilter = null, double? minHeight = null, double? maxHeight = null)
        {
            var obstaclesQuery = _context.Obstacles.AsQueryable();

            // Status filter
            if (!string.IsNullOrWhiteSpace(statusFilter))
            {
                var normalizedFilter = statusFilter.Trim();
                if (string.Equals(normalizedFilter, "Rejected", StringComparison.OrdinalIgnoreCase))
                {
                    normalizedFilter = "Declined";
                }
                obstaclesQuery = obstaclesQuery.Where(o => o.Status == normalizedFilter);
            }

            // Organization filter
            if (!string.IsNullOrWhiteSpace(organizationFilter))
            {
                obstaclesQuery = obstaclesQuery.Where(o => o.Organization == organizationFilter);
            }

            // Obstacle type filter
            if (!string.IsNullOrWhiteSpace(obstacleTypeFilter))
            {
                obstaclesQuery = obstaclesQuery.Where(o => o.ObstacleType == obstacleTypeFilter);
            }

            // Search term filter
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                obstaclesQuery = obstaclesQuery.Where(o => o.ObstacleName != null && o.ObstacleName.Contains(searchTerm));
            }

            // Height filters
            if (minHeight.HasValue)
            {
                obstaclesQuery = obstaclesQuery.Where(o => o.ObstacleHeight >= minHeight.Value);
            }
            if (maxHeight.HasValue)
            {
                obstaclesQuery = obstaclesQuery.Where(o => o.ObstacleHeight <= maxHeight.Value);
            }

            // Date range filter (end date inclusive by adding a day)
            if (startDate.HasValue)
            {
                obstaclesQuery = obstaclesQuery.Where(o => o.SubmittedDate >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                obstaclesQuery = obstaclesQuery.Where(o => o.SubmittedDate < endDate.Value.AddDays(1));
            }

            var obstacles = await obstaclesQuery
                .OrderByDescending(o => o.SubmittedDate)
                .ToListAsync();

            var viewModel = new ObstacleListViewModel
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

        /// <summary>
        /// Shows obstacle details. Authorization ensures only authenticated roles can view. Passes role/unit
        /// flags via ViewBag for UI adjustments.
        /// </summary>
        [Authorize(Roles = "Pilot,Caseworker,Admin")]
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var obstacle = await _context.Obstacles.FindAsync(id);

            if (obstacle == null)
            {
                return NotFound();
            }

            ViewBag.IsPilot = IsPilot();
            ViewBag.UsesFeet = IsPilot();

            return View(obstacle);
        }

        /// <summary>
        /// GET edit action. Pilots can only edit their own obstacles enforced by SubmittedBy check. Others
        /// (Caseworker/Admin) can edit any. Returns 403 when pilot tries to edit another user's entry.
        /// </summary>
        [Authorize(Roles = "Pilot,Caseworker,Admin")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var obstacle = await _context.Obstacles.FindAsync(id);

            if (obstacle == null)
            {
                return NotFound();
            }

            // SECURITY: Pilots can only edit their own reports.
            if (User.IsInRole("Pilot") && obstacle.SubmittedBy != User.Identity?.Name)
            {
                return Forbid(); // Return 403 Access Denied
            }

            ViewBag.IsPilot = IsPilot();
            ViewBag.UsesFeet = IsPilot();

            return View(obstacle);
        }

        /// <summary>
        /// POST edit action. Uses Bind to prevent overposting. Re-validates ownership for pilots to ensure
        /// server-side enforcement even if UI is tampered with. ModelState errors re-render view.
        /// </summary>
        [Authorize(Roles = "Pilot,Caseworker,Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id, ObstacleName, ObstacleHeight, ObstacleDescription, Longitude, Latitude, LineGeoJson")] ObstacleData obstacledata)
        {
            if (id != obstacledata.Id) // Ensure the ID in the URL matches the ID in the form data
            {
                return NotFound();
            }

            var obstacleToUpdate = await _context.Obstacles.FindAsync(id);

            if (obstacleToUpdate == null)
            {
                return NotFound();
            }

            // SECURITY: Pilots can only edit their own reports.
            if (User.IsInRole("Pilot") && obstacleToUpdate.SubmittedBy != User.Identity?.Name)
            {
                return Forbid(); // Return 403 Access Denied
            }

            ModelState.Remove("Organization");

            if (!ModelState.IsValid)
            {
                ViewBag.IsPilot = IsPilot();
                ViewBag.UsesFeet = IsPilot();
                // Return the original entity from the database, not the invalid one from the model binder.
                return View(obstacleToUpdate);
            }

            try // Try to update the obstacle in the database
            {
                // Apply the changes from the bound model
                obstacleToUpdate.ObstacleName = obstacledata.ObstacleName;
                obstacleToUpdate.ObstacleHeight = obstacledata.ObstacleHeight;
                obstacleToUpdate.ObstacleDescription = obstacledata.ObstacleDescription ?? "";
                obstacleToUpdate.Longitude = obstacledata.Longitude;
                obstacleToUpdate.Latitude = obstacledata.Latitude;
                obstacleToUpdate.LineGeoJson = obstacledata.LineGeoJson;

                // Set modification metadata for audit trail
                obstacleToUpdate.LastModifiedBy = User.Identity?.Name ?? "Unknown";
                obstacleToUpdate.LastModifiedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException) // Handle concurrency issues
            {
                if (!await ObstacleExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return RedirectToAction(nameof(List));
        }

        /// <summary>
        /// Approves an obstacle. Restricted to Caseworker/Admin. TempData conveys notification styling
        /// after redirect to the pending list.
        /// </summary>
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

            // Update obstacle status and approval metadata
            obstacle.Status = "Approved";
            obstacle.ApprovedBy = User.Identity?.Name ?? "Unknown";
            obstacle.ApprovedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["NotificationMessage"] = "Obstacle was approved";
            TempData["NotificationType"] = "success";

            return RedirectToAction(nameof(List), new { statusFilter = "Pending" });
        }

        /// <summary>
        /// Declines an obstacle with an optional reason. Restricted to Caseworker/Admin. TempData used for
        /// PRG messaging.
        /// </summary>
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

        /// <summary>
        /// Moves an obstacle back to Pending for re-evaluation. Restricted to Caseworker/Admin.
        /// </summary>
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

        /// <summary>
        /// Deletes an obstacle. Only Admin can perform this action. Anti-forgery protects the POST.
        /// </summary>
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

            _context.Obstacles.Remove(obstacle);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(List));
        }

        /// <summary>
        /// Helper to check for obstacle existence during concurrency handling.
        /// </summary>
        private async Task<bool> ObstacleExists(int id)
        {
            return await _context.Obstacles.AnyAsync(e => e.Id == id);
        }
    }
}
