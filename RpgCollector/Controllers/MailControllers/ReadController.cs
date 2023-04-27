using Microsoft.AspNetCore.Mvc;

namespace RpgCollector.Controllers.MailControllers
{
    public class ReadController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
