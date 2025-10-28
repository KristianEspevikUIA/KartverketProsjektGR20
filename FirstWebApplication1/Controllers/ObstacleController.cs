using FirstWebApplication1.Data;
using FirstWebApplication1.Models; // Importerer modellene (her: ObstacleData)
using Microsoft.AspNetCore.Mvc; // Gir tilgang til ASP.NET Core MVC-funksjonalitet

namespace FirstWebApplication1.Controllers
{
    // Kontroller for håndtering av skjema knyttet til hindringsdata (ObstacleData)
    public class ObstacleController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ObstacleController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet] // Håndterer GET-forespørsel når siden lastes første gang
        public IActionResult DataForm()
        {
            return View(); // Viser DataForm-view uten modell (tomt skjema)
        }

        // Blir kalt etter at vi trykker på "Submit Data" knapp i DataForm viewet
        [HttpPost] // Håndterer POST-forespørsel etter innsending av skjema
        public async Task<IActionResult> DataForm(ObstacleData obstacledata)
        {
            if (!ModelState.IsValid) // Sjekker om modellen (inputdata) er gyldig i henhold til valideringsregler
            {
                // Viser DataForm på nytt dersom modellen ikke er gyldig
                return View(obstacledata);
            }

            _context.Obstacles.Add(obstacledata);
            await _context.SaveChangesAsync();

            // Gyldig data: vis Overview med modellen
            return View("Overview", obstacledata); // Sender brukeren til Overview-viewet med innsendte data
        }
    }
}
