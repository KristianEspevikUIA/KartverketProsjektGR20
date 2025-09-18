using FirstWebApplication1.Models;
using Microsoft.AspNetCore.Mvc;

namespace FirstWebApplication1.Controllers
{
    public class ObstacleController : Controller
    {
        [HttpGet]
        public IActionResult DataForm()
        {
            return View();
        }

        // Blir kalt etter at vi trykker på "Submit Data" knapp i DataForm viewet
        [HttpPost]
        public IActionResult DataForm(ObstacleData obstacledata)
        {
            if (!ModelState.IsValid)
            {
                // Viser DataForm på nytt dersom modellen ikke er gyldig
                return View(obstacledata);
            }

            // Gyldig data: vis Overview med modellen
            return View("Overview", obstacledata);
        }
    }
}
