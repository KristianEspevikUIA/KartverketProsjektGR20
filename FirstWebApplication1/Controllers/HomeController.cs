using System.Diagnostics; // Gir tilgang til diagnostikkverkt�y, f.eks. sporing av aktivitet
using FirstWebApplication1.Models; // Importerer modeller fra prosjektet
using Microsoft.AspNetCore.Mvc; // Gir tilgang til ASP.NET Core MVC-funksjoner
using MySqlConnector; // MySQL/MariaDB-connector for databasekobling

namespace FirstWebApplication1.Controllers
{
    // HomeController handles requests to standard pages (Home/Privacy/Error)
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger; // loggs for the controller
        private readonly string _connectionString; // holds the database connection string  
        // Constructor that retrieves the connection string from the configuration (appsettings.json)
        public HomeController(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        // return a standard view for the Home page
        public IActionResult Index()
        {
            return View();
        }

        // return a standard view for the Privacy page
        public IActionResult Privacy()
        {
            return View();
        }

        // return a standard view for the Error page, with RequestId for troubleshooting
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)] // No caching
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
