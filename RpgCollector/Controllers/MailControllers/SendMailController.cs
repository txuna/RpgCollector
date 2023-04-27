using Microsoft.AspNetCore.Mvc;

namespace RpgCollector.Controllers.MailControllers
{
    public class SendMailController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
