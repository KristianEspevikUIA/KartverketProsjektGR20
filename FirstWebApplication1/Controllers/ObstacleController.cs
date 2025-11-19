using FirstWebApplication1.Data;
using FirstWebApplication1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
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

            // --- CORRECTED CODE START ---
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
            // --- CORRECTED CODE END ---


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
                if (TempData["ObstacleType"] != null)
                {
                    obstacledata.ObstacleType = TempData["ObstacleType"].ToString();
                    
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

                if (!ModelState.IsValid)
                {
                    ViewBag.IsPilot = IsPilot();
                    ViewBag.UsesFeet = IsPilot();
                    return View(obstacledata);
                }

                // Set status to Pending
                obstacledata.Status = "Pending";
                obstacledata.SubmittedBy = User.Identity?.Name ?? "Unknown";
                obstacledata.SubmittedDate = DateTime.UtcNow;

                _context.Obstacles.Add(obstacledata);
                await _context.SaveChangesAsync();

                // Pass user role to overview
                ViewBag.IsPilot = IsPilot();
                ViewBag.UsesFeet = IsPilot();

                return View("Overview", obstacledata);
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                ModelState.AddModelError("", $"Error saving obstacle: {innerMessage}");

                ViewBag.IsPilot = IsPilot();
                ViewBag.UsesFeet = IsPilot();

                return View(obstacledata);
            }
        }

        [Authorize(Roles = "Pilot,Registerfører,Admin")]
        [HttpGet]
        public async Task<IActionResult> List()
        {
            var obstacles = await _context.Obstacles
                .OrderByDescending(o => o.SubmittedDate)
                .ToListAsync();

            ViewBag.IsPilot = IsPilot();
            ViewBag.UsesFeet = IsPilot();

            return View(obstacles);
        }

        [Authorize(Roles = "Pilot,Registerfører,Admin")]
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

        [Authorize(Roles = "Pilot,Registerfører,Admin")]
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

        [Authorize(Roles = "Pilot,Registerfører,Admin")]
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

        [Authorize(Roles = "Registerfører,Admin")]
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

            return RedirectToAction(nameof(Details), new { id });
        }

        [Authorize(Roles = "Registerfører,Admin")]
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
