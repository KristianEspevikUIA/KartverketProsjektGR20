using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FirstWebApplication1.Data;

namespace FirstWebApplication1.Controllers
{
    [Authorize(Roles = "Admin")] // Kun Admin slipper inn her!
    public class OrganizationController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrganizationController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Organization/Index
        public async Task<IActionResult> Index()
        {
            // Henter en liste over unike organisasjoner og teller hvor mange hindre hver har
            var orgStats = await _context.Obstacles
                .Where(o => o.Organization != null && o.Organization != "")
                .GroupBy(o => o.Organization)
                .Select(g => new OrganizationViewModel 
                { 
                    Name = g.Key, 
                    Count = g.Count() 
                })
                .ToListAsync();

            return View(orgStats);
        }

        // POST: /Organization/Delete
        // Dette sletter ikke hindrene, men fjerner organisasjonsnavnet fra dem (opprydding)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string orgName)
        {
            var obstaclesToUpdate = await _context.Obstacles
                .Where(o => o.Organization == orgName)
                .ToListAsync();

            foreach (var obstacle in obstaclesToUpdate)
            {
                obstacle.Organization = null; // Nullstiller navnet
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }

    // Enkel ViewModel for denne siden
    public class OrganizationViewModel
    {
        public string Name { get; set; }
        public int Count { get; set; }
    }
}