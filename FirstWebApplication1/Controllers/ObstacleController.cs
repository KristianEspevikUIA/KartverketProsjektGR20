using FirstWebApplication1.Data;
using FirstWebApplication1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

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

        // GET: Edit obstacle - Pilot, Registerfører, and Admin can edit
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

        // POST: Update obstacle
        [Authorize(Roles = "Pilot,Registerfører,Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ObstacleData obstacledata)
        {
            if (id != obstacledata.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(obstacledata);
            }

            try
            {
                obstacledata.LastModifiedBy = User.Identity?.Name ?? "Unknown";
                obstacledata.LastModifiedDate = DateTime.UtcNow;

                _context.Update(obstacledata);
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

            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: Approve obstacle - Only Registerfører and Admin
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

        // DELETE: Delete obstacle - Only Admin
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