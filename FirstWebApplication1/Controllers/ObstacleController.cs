using FirstWebApplication1.Data;
using FirstWebApplication1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Text.Json;

namespace FirstWebApplication1.Controllers
{
    [EnableRateLimiting("Fixed")]
    public class ObstacleController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ObstacleController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Helper method to check if user is a pilot
        private bool IsPilot()
        {
            return User.IsInRole("Pilot");
        }

        // STEP 1: Select obstacle type
        [Authorize]
        [HttpGet]
        public IActionResult SelectType()
        {
            return View(new ObstacleTypeViewModel());
        }

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

            TempData["ObstacleType"] = model.SelectedType;
            return RedirectToAction(nameof(DataForm));
        }

        // STEP 2: Fill in details
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
                .AsNoTracking()
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

            // Pass user role info to view
            ViewBag.IsPilot = IsPilot();
            ViewBag.UsesFeet = IsPilot();

            return View(obstacleData);
        }

        // STEP 3: Submit and show overview
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

                // Remove validation for optional/auto-generated fields
                ModelState.Remove("ObstacleType");
                ModelState.Remove("ObstacleName");
                ModelState.Remove("ObstacleDescription");

                var isPilot = IsPilot();
                var usesFeetPreference = useFeet ?? isPilot;


                if (!ModelState.IsValid)
                {
                    ViewBag.IsPilot = isPilot;
                    ViewBag.UsesFeet = usesFeetPreference;
                    return View(obstacledata);
                }

                // Set status to Pending
                obstacledata.Status = "Pending";
                obstacledata.SubmittedBy = User.Identity?.Name ?? "Unknown";
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

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Overview(int id, bool? useFeet)
        {
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

        [Authorize(Roles = "Pilot,Caseworker,Admin")]
        [HttpGet]
        public async Task<IActionResult> List(string? statusFilter = null, string? searchTerm = null, DateTime? startDate = null, DateTime? endDate = null, string? obstacleTypeFilter = null)
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

            // Date range filter
            if (startDate.HasValue)
            {
                obstaclesQuery = obstaclesQuery.Where(o => o.SubmittedDate >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                // Add one day to the end date to include all of that day
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
                ObstacleTypeFilter = obstacleTypeFilter
            };

            ViewBag.IsPilot = IsPilot();
            ViewBag.UsesFeet = IsPilot();

            return View(viewModel);
        }

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

        [Authorize(Roles = "Pilot,Caseworker,Admin")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
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

        [Authorize(Roles = "Pilot,Caseworker,Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id, ObstacleName, ObstacleHeight, ObstacleDescription, Longitude, Latitude, LineGeoJson")] ObstacleData obstacledata)
        {
            if (id != obstacledata.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                // If model state is invalid, we must reload the full entity to pass back to the view
                var fullObstacle = await _context.Obstacles.AsNoTracking().FirstOrDefaultAsync(o => o.Id == id);
                if (fullObstacle == null) return NotFound();

                // We update the full entity with the user's failed changes to show them back
                fullObstacle.ObstacleName = obstacledata.ObstacleName;
                fullObstacle.ObstacleHeight = obstacledata.ObstacleHeight;
                // etc. for other bound properties

                ViewBag.IsPilot = IsPilot();
                ViewBag.UsesFeet = IsPilot();
                return View(fullObstacle);
            }

            try
            {
                var obstacleToUpdate = await _context.Obstacles.FindAsync(id);
                if (obstacleToUpdate == null)
                {
                    return NotFound();
                }

                // Apply the changes from the bound model
                obstacleToUpdate.ObstacleName = obstacledata.ObstacleName;
                obstacleToUpdate.ObstacleHeight = obstacledata.ObstacleHeight;
                obstacleToUpdate.ObstacleDescription = obstacledata.ObstacleDescription ?? "";
                obstacleToUpdate.Longitude = obstacledata.Longitude;
                obstacleToUpdate.Latitude = obstacledata.Latitude;
                obstacleToUpdate.LineGeoJson = obstacledata.LineGeoJson;

                // Set modification metadata
                obstacleToUpdate.LastModifiedBy = User.Identity?.Name ?? "Unknown";
                obstacleToUpdate.LastModifiedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await ObstacleExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return RedirectToAction(nameof(List));
        }

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

            obstacle.Status = "Approved";
            obstacle.ApprovedBy = User.Identity?.Name ?? "Unknown";
            obstacle.ApprovedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["NotificationMessage"] = "Obstacle was approved";
            TempData["NotificationType"] = "success";

            return RedirectToAction(nameof(List), new { statusFilter = "Pending" });
        }

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

        private async Task<bool> ObstacleExists(int id)
        {
            return await _context.Obstacles.AnyAsync(e => e.Id == id);
        }
    }
}