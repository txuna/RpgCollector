using Microsoft.AspNetCore.Mvc;

namespace RpgCollector.Controllers.MailControllers
{
    public class ReadMailController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
