using Microsoft.AspNetCore.Mvc;
using RpgCollector.Services;

namespace RpgCollector.Controllers
{
    public class PostController : Controller
    {
        IPostService _postService; 
        public PostController(IPostService postService) 
        {
            _postService = postService;
        }
        /*
         * 지정된 userId에게 우편을 보냄 이때 우편은 아이템이 동봉될 수 있음 
         * 전송권한은 관리자 권한만 가능
         */
        [Route("/Game/SendPost")]
        [HttpPost]
        public Task<IActionResult> SendPost()
        {
            return View();
        }
        /*
         * 사용자가 우편함을 열었을 때 호출되는 API 
         * 사용자가 처음 우편함을 열었는지에 대한 IsFirstOpen값과 열었다면 몇번째 페이지를 원하는지에 대한 PageNumber 값을 포함
         * 반환값으로는 PageNumber에 맞는 위치에서의 20개를 준다.
         * 또한 반환값으로 TotalPageNumber을 넘겨 클라이언트에게 PageNumber가 몇번까지 있는지 인지시킨다. 
         */
        [Route("/Game/OpenPost")]
        [HttpPost]
        public Task<IActionResult> OpenPost()
        {
            return View();
        }

        /*
         * 우편을 읽을 수 있는 API
         * 우편을 읽을 수 있으며 아이템이 동봉된 경우 수령을 선택할 수 있다. 
         */
        [Route("/Game/ReadPost")]
        [HttpPost]
        public Task<IActionResult> ReadPost()
        {
            return View();
        }

        /*
         * 우편에 동봉된 아이템을 얻는 API 
         * 만약 우편을 읽지 않는 경우 우편을 읽었음을 표시해준다.
         */
        [Route("/Game/GetPostItem")]
        [HttpPost]
        public Task<IActionResult> GetPostItem()
        {
            return View();
        }
    }
}
