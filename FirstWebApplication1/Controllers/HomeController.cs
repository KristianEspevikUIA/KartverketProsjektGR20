using System.Diagnostics;
using FirstWebApplication1.Models;
using Microsoft.AspNetCore.Mvc;

namespace FirstWebApplication1.Controllers
{
    /// <summary>
    /// Handles public landing pages (Index/Privacy) and error display. Acts as the MVC controller entry for
    /// unauthenticated users and routes to shared views.
    /// </summary>
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly string _connectionString;

        /// <summary>
        /// Constructor resolves configuration for potential future DB usage and logging for diagnostics.
        /// </summary>
        /// <param name="config">Application configuration source.</param>
        public HomeController(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        /// <summary>
        /// Landing page for the application.
        /// </summary>
        /// <returns>Razor view for Index.</returns>
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Privacy information page.
        /// </summary>
        /// <returns>Razor view for Privacy.</returns>
        public IActionResult Privacy()
        {
            return View();
        }

        /// <summary>
        /// Error endpoint used by exception handler middleware. ResponseCache disabled to avoid caching
        /// sensitive error information; shows a correlation id for support/debugging.
        /// </summary>
        /// <returns>Error view with <see cref="ErrorViewModel"/>.</returns>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
