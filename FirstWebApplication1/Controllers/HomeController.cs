using System.Diagnostics; // Gir tilgang til diagnostikkverktøy, f.eks. sporing av aktivitet
using FirstWebApplication1.Models; // Importerer modeller fra prosjektet
using Microsoft.AspNetCore.Mvc; // Gir tilgang til ASP.NET Core MVC-funksjoner
using MySqlConnector; // MySQL/MariaDB-connector for databasekobling

namespace FirstWebApplication1.Controllers
{
    // HomeController håndterer forespørsler til standard sider (Home/Privacy/Error)
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger; // Logger for feilsøking og logging
        private readonly string _connectionString; // Holder på database-tilkoblingsstreng

        // Konstruktør som henter tilkoblingsstrengen fra konfigurasjonen (appsettings.json)
        public HomeController(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        /* 
        // Eksempelmetode for å teste MariaDB-tilkobling (kommentert ut)
        public async Task<IActionResult> Index()
        {
            try
            {
                await using var conn = new MySqlConnection(_connectionString); // Oppretter DB-forbindelse
                await conn.OpenAsync(); // Åpner asynkron forbindelse
                return Content("Connected to MariaDB successfully!"); // Tilbakemelding hvis OK
            }
            catch (Exception ex)
            {
                return Content("Failed to connect to MariaDB: " + ex.Message); // Tilbakemelding ved feil
            }
        }

        // Alternativ konstruktør med logger (kommentert ut)
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }
        */

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

        // Returnerer visning for feil, med RequestId for feilsøking
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)] // Ingen caching
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
