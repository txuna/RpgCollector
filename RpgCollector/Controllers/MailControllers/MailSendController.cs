using Microsoft.AspNetCore.Mvc;

namespace RpgCollector.Controllers.MailControllers
{
    public class MailSendController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
