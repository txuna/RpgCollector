using Microsoft.AspNetCore.Mvc;

namespace RpgCollector.Controllers.PackageControllers
{
    public class PackageBuyController : Controller
    {

        [Route("/Package/Buy")]
        [HttpPost]
        public IActionResult Index()
        {
            return View();
        }
    }
}
