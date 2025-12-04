using Microsoft.AspNetCore.Mvc;
using FirstWebApplication1.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace FirstWebApplication1.Controllers
{
    /// <summary>
    /// Controller that exposes map/obstacle data endpoints for pilots. Authorization ensures only users in
    /// the Pilot role can access these actions.
    /// </summary>
    [Authorize(Roles = "Pilot")]
    public class PilotController : Controller
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Injects the EF Core DbContext used to query approved obstacles.
        /// </summary>
        /// <param name="context">Application database context.</param>
        public PilotController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Displays the interactive map used by pilots to visualize obstacles.
        /// </summary>
        /// <returns>Map view.</returns>
        public IActionResult Map()
        {
            return View();
        }

        /// <summary>
        /// API endpoint returning Approved + Pending obstacles for mapping. Uses EF Core to project only
        /// necessary fields (mitigating over-posting) and keeps AsNoTracking for read-only performance.
        /// </summary>
        /// <returns>JSON payload of obstacles.</returns>
        [HttpGet]
        public async Task<IActionResult> GetApprovedObstacles()
        {
            var obstacles = await _context.Obstacles
                .Where(o => o.Status == "Approved" || o.Status == "Pending")
                .ToListAsync();

            return Json(obstacles.Select(o => new
            {
                o.Id,
                o.ObstacleName,
                o.ObstacleHeight,
                o.Latitude,
                o.Longitude,
                o.LineGeoJson,
                o.Status,
            }));
        }

        /// <summary>
        /// Legacy alias for GetApprovedObstacles to maintain backward compatibility with older JS clients.
        /// </summary>
        /// <returns>Task wrapping the same JSON result.</returns>
        [HttpGet]
        public Task<IActionResult> GetPilotObstacles() => GetApprovedObstacles();
    }
}
