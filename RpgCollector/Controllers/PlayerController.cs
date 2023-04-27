using Microsoft.AspNetCore.Mvc;

namespace RpgCollector.Controllers
{
    public class PlayerController : Controller
    {
        public PlayerController()
        {

        }
        /*
         * 로그인 성공시 요청받는 메서드로써 플레이어의 게임데이터를 전송한다.
         * 전송되는 정보 
         * 1. 해당 플레이어 데이터 
         * 2. 해당 플레이어 아이템 인벤토리
         */
        [Route("/Game/Player")]
        [HttpPost]
        public async Task<IActionResult> GetPlayerData()
        {
            return View();
        }
    }
}
