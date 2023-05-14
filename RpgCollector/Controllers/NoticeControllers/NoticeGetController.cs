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

    [Route("/Notice")]
    [HttpPost]
    public async Task<NoticeGetResponse> Notice()
    {
        Notice[]? result = await _memoryDB.GetAllNotice();

        if (result == null)
        {
            _logger.ZLogError("Failed Fetch Notice In Redis");

            return new NoticeGetResponse
            {
                Error = ErrorCode.FailedConnectRedis,
            };
        }

        return new NoticeGetResponse
        {
            Error = ErrorCode.None,
            NoticeList = result
        };
    }
}
