using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RpgCollector.RequestResponseModel.NoticeGetModel;
using RpgCollector.Services;
using RpgCollector.RequestResponseModel;
using StackExchange.Redis;
using System;
using System.Text.Json;
using ZLogger;
using RpgCollector.Models.NoticeModel;

namespace RpgCollector.Controllers.NoticeControllers;

[ApiController]
[Route("[controller]")]
public class NoticeGetController : Controller
{
    INoticeMemoryDB _memoryDB;
    ILogger<NoticeGetController> _logger;

    public NoticeGetController(INoticeMemoryDB memoryDB, ILogger<NoticeGetController> logger)
    {
        _memoryDB = memoryDB;
        _logger = logger;
    }

    /*
     * Redis에 저장된 공지사항을 리스트형식으로 뿌려준다. 
     */
    [Route("/Notice")]
    [HttpPost]
    public async Task<NoticeGetResponse> Notice()
    {
        Notice[]? result = await _memoryDB.GetAllNotice();
        string userName = HttpContext.Request.Headers["User-Name"];

        _logger.ZLogInformation($"[{userName}] Request 'Get Notice'");

        if (result == null)
        {
            _logger.ZLogError("Failed Fetch Notice In Redis");

            return new NoticeGetResponse
            {
                Error = ErrorState.FailedConnectRedis,
            };
        }

        _logger.ZLogInformation("Success Fetch Notice In Redis");

        return new NoticeGetResponse
        {
            Error = ErrorState.None,
            NoticeList = result
        };
    }
}
