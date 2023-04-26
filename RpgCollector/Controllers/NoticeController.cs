using Microsoft.AspNetCore.Mvc;
using RpgCollector.Services;

namespace RpgCollector.Controllers
{
    public class NoticeController : Controller
    {
        private INoticeService _noticeService; 

        public NoticeController(INoticeService noticeService) 
        {
            _noticeService = noticeService;
        }

        /*
         * Redis에 저장된 공지사항을 리스트형식으로 뿌려준다. 
         */
        [Route("/Game/Notice")]
        [HttpGet]
        public async Task<IActionResult> Notice()
        {
            var (success, content) = await _noticeService.GetAllNotice(); 
            if(!success)
            {
                return BadRequest(content); 
            }
            return Json(content);
        }
    }
}
