using System.Diagnostics; // Gir tilgang til diagnostikkverkt�y, f.eks. sporing av aktivitet
using FirstWebApplication1.Models; // Importerer modeller fra prosjektet
using Microsoft.AspNetCore.Mvc; // Gir tilgang til ASP.NET Core MVC-funksjoner
using MySqlConnector; // MySQL/MariaDB-connector for databasekobling

namespace FirstWebApplication1.Controllers
{
    // HomeController h�ndterer foresp�rsler til standard sider (Home/Privacy/Error)
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger; // Logger for feils�king og logging
        private readonly string _connectionString; // Holder p� database-tilkoblingsstreng

        // Konstrukt�r som henter tilkoblingsstrengen fra konfigurasjonen (appsettings.json)
        public HomeController(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        // Returnerer standard visning for Home-siden
        public IActionResult Index()
        {
            return View();
        }

        // Returnerer visning for Privacy-siden
        public IActionResult Privacy()
        {
            return View();
        }

        // Returnerer visning for feil, med RequestId for feils�king
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)] // Ingen caching
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
