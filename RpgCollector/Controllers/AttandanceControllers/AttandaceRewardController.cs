using Microsoft.AspNetCore.Mvc;
using RpgCollector.Services;

namespace RpgCollector.Controllers.AttandanceControllers
{
    [ApiController]
    public class AttandaceRewardController : Controller
    {
        IMailboxAccessDB _mailboxAccessDB;

        public AttandaceRewardController(IMailboxAccessDB mailboxAccessDB)
        {
            _mailboxAccessDB = mailboxAccessDB;
        }

        /*
        * 서버시간을 기준으로 사용자의 출석을 진행하는 API 
        * - 연속 출석일수에 맞춰 보상을 지급한다. 
        * - 보상은 아이템이 동봉된 메일을 전송한다.
        */
        [Route("/Attandace")]
        [HttpPost]
        public IActionResult Index()
        {
            return View();
        }
    }
}
