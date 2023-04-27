using Microsoft.AspNetCore.Mvc;

namespace RpgCollector.Controllers
{
    public class MasterDataController : Controller
    {
        public MasterDataController() 
        {

        }

        /*
         * 게임의 마스터 데이터를 넘겨준다.
         */ 
        public async Task<IActionResult> Index()
        {
            return View();
        }
    }
}
