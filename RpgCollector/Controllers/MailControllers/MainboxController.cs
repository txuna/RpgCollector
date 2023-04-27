using Microsoft.AspNetCore.Mvc;

namespace RpgCollector.Controllers.MailControllers
{
    public class MainboxController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
