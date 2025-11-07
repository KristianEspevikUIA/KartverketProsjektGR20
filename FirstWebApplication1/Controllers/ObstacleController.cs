using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FirstWebApplication1.Data;
using FirstWebApplication1.Models;

namespace FirstWebApplication1.Controllers
{
    // If you previously had any attributes like rate-limiting, keep them;
    // the original snapshot included [EnableRateLimiting("Fixed")] in some versions.
    public class ObstacleController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ObstacleController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: DataForm - Only authenticated users can create obstacles
        [Authorize]
        [HttpGet]
        public IActionResult DataForm()
        {
            return View();
        }

        // POST: DataForm - Submit new obstacle
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DataForm(ObstacleData obstacledata)
        {
            if (!ModelState.IsValid)
            {
                return View(obstacledata);
            }

            // Set status to Pending for new submissions
            obstacledata.Status = "Pending";
            obstacledata.SubmittedBy = User.Identity?.Name ?? "Unknown";
            obstacledata.SubmittedDate = DateTime.UtcNow;

            _context.Obstacles.Add(obstacledata);
            await _context.SaveChangesAsync();

            return View("Overview", obstacledata);
        }

        // GET: List all obstacles - Pilot, Registerfører, and Admin can view
        [Authorize(Roles = "Pilot,Registerfører,Admin")]
        [HttpGet]
        public async Task<IActionResult> List()
        {
            var obstacles = await _context.Obstacles
                .OrderByDescending(o => o.SubmittedDate)
                .ToListAsync();

            return View(obstacles);
        }

        // GET: View single obstacle details
        [Authorize(Roles = "Pilot,Registerfører,Admin")]
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var obstacle = await _context.Obstacles.FindAsync(id);

            if (obstacle == null)
            {
                return NotFound();
            }

            return View(obstacle);
        }

        // POST: Decline obstacle - Only Registerfører and Admin
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

        // NEW: Approve obstacle - Registerfører and Admin
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

        // DELETE: Delete obstacle - allow Registerfører and Admin to delete reported obstacles
        [Authorize(Roles = "Registerfører,Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var obstacle = await _context.Obstacles.FindAsync(id);
            if (obstacle == null)
            {
                return NotFound();
            }

            // Remove the obstacle (hard delete).
            // If you prefer soft-delete, change to mark a flag and filter it from lists instead.
            _context.Obstacles.Remove(obstacle);
            await _context.SaveChangesAsync();

            // After deletion, redirect to list page
            return RedirectToAction(nameof(List));
        }

        private async Task<bool> ObstacleExists(int id)
        {
            return await _context.Obstacles.AnyAsync(e => e.Id == id);
        }


        // Add these methods inside the ObstacleController class

        // GET: /Obstacle/Edit/5
        [Authorize(Roles = "Pilot,Registerfører,Admin")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var obstacle = await _context.Obstacles.FindAsync(id);
            if (obstacle == null)
            {
                return NotFound();
            }

            return View(obstacle);
        }

        // POST: /Obstacle/Edit/5
        [Authorize(Roles = "Pilot,Registerfører,Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, FirstWebApplication1.Models.ObstacleData obstacledata)
        {
            if (id != obstacledata.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                // Return the same view so validation errors are shown
                return View(obstacledata);
            }

            var obstacle = await _context.Obstacles.FindAsync(id);
            if (obstacle == null)
            {
                return NotFound();
            }

            // Update only the editable fields to avoid overwriting important metadata.
            obstacle.ObstacleName = obstacledata.ObstacleName;
            obstacle.ObstacleDescription = obstacledata.ObstacleDescription;
            obstacle.ObstacleHeight = obstacledata.ObstacleHeight;
            obstacle.LineGeoJson = obstacledata.LineGeoJson;
            obstacle.Latitude = obstacledata.Latitude;
            obstacle.Longitude = obstacledata.Longitude;

            obstacle.LastModifiedBy = User.Identity?.Name ?? "Unknown";
            obstacle.LastModifiedDate = DateTime.UtcNow;

            // Keep SubmittedBy/SubmittedDate/Status/etc. as they were.
            try
            {
                _context.Update(obstacle);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Obstacles.AnyAsync(e => e.Id == id))
                {
                    return NotFound();
                }
                throw;
            }

            return RedirectToAction(nameof(Details), new { id = obstacle.Id });
        }
    }
}