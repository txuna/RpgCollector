using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RpgCollector.Models;
using RpgCollector.RequestResponseModel.NoticeGetModel;
using RpgCollector.Services;
using RpgCollector.RequestResponseModel;
using StackExchange.Redis;
using System;
using System.Text.Json;

namespace RpgCollector.Controllers;

[ApiController]
[Route("[controller]")]
public class NoticeGetController : Controller
{
    INoticeMemoryDB _memoryDB;

    public NoticeGetController(INoticeMemoryDB memoryDB)
    {
        _memoryDB = memoryDB;
    }

    /*
     * Redis에 저장된 공지사항을 리스트형식으로 뿌려준다. 
     */
    [Route("/Notice")]
    [HttpPost]
    public async Task<NoticeGetResponse> Notice()
    {
        Notice[]? result = await _memoryDB.GetAllNotice();

        if(result == null)
        {
            return new NoticeGetResponse
            {
                Error = ErrorState.FailedConnectRedis,
            };
        }
        return new NoticeGetResponse
        {
            Error = ErrorState.None,
            NoticeList = result
        };
    }
}
