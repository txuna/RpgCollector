using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RpgCollector.Models;
using RpgCollector.ResponseModels;
using RpgCollector.Services;
using StackExchange.Redis;
using System;
using System.Text.Json;

namespace RpgCollector.Controllers
{
    public class NoticeController : Controller
    {
        INoticeMemoryDB _memoryDB;

        public NoticeController(INoticeMemoryDB memoryDB)
        {
            _memoryDB = memoryDB;
        }

        /*
         * Redis에 저장된 공지사항을 리스트형식으로 뿌려준다. 
         */
        [Route("/Notice")]
        [HttpPost]
        public async Task<JsonResult> Notice()
        {
            Notice[]? result = await _memoryDB.GetAllNotice();

            if(result == null)
            {
                return Json(new FailResponse
                {
                    Success = false,
                    Message = "Failed Fetch Notice"
                });
            }

            return Json(new NoticeResponse
            {
                Success = true,
                NoticeList = result
            });
        }
    }
}
