using Microsoft.AspNetCore.Mvc;
using FirstWebApplication1.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace FirstWebApplication1.Controllers
{
    [Authorize(Roles = "Pilot")]
    public class PilotController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PilotController(ApplicationDbContext context)
        {
            _context = context;
        }

        // === View: the map page ===
        public IActionResult Map()
        {
            return View();
        }

        // === API ENDPOINT: returns APPROVED obstacles ===
        [HttpGet]
        public async Task<IActionResult> GetPilotObstacles()
        {
            var approved = await _context.Obstacles
                .Where(o => o.Status == "Approved" || o.Status == "Pending")
                .ToListAsync();

            return Json(approved.Select(o => new {
                o.Id,
                o.ObstacleName,
                o.ObstacleHeight,
                o.Latitude,
                o.Longitude,
                o.LineGeoJson,
                o.Status,
            }));
        }
    }
}