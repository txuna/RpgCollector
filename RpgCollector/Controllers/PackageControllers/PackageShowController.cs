using Microsoft.AspNetCore.Mvc;

namespace RpgCollector.Controllers.PackageControllers
{
    public class PackageShowController : Controller
    {
        [Route("/Package/Show")]
        [HttpPost]
        public IActionResult Index()
        {
            return View();
        }
    }
}
