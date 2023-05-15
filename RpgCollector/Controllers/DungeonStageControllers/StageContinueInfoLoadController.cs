using Microsoft.AspNetCore.Mvc;
using RpgCollector.Models.StageModel;
using RpgCollector.RequestResponseModel;
using RpgCollector.RequestResponseModel.DungeonStageReqRes;
using RpgCollector.Services;
using ZLogger;

namespace RpgCollector.Controllers.DungeonStageControllers;

[ApiController]
public class StageContinueInfoLoadController : Controller
{
    IRedisMemoryDB _redisMemoryDB;
    ILogger<StageContinueInfoLoadController> _logger;
    public StageContinueInfoLoadController(IRedisMemoryDB redisMemoryDB, ILogger<StageContinueInfoLoadController> logger)
    {
        _redisMemoryDB = redisMemoryDB;
        _logger = logger;
    }

    /*
     *  클라이언트 로그인 리스폰스에 user playing이라는 플래그가 포함된다면 아래 API 요청
     */
    [Route("/Stage/Continue")]
    [HttpPost]
    public async Task<StagePlayingInfoLoadResponse> Post(StagePlayingInfoLoadRequest stagePlayingInfoLoadRequest)
    {
        int userId = Convert.ToInt32(HttpContext.Items["User-Id"]);
        string authToken = Convert.ToString(HttpContext.Items["Auth-Token"]);
        string userName = stagePlayingInfoLoadRequest.UserName;

        _logger.ZLogDebug($"[{userId}] Request /Stage/Continue");

        RedisPlayerStageInfo? redisPlayerStageInfo = await LoadStagePlayerInfo(userName);
        if (redisPlayerStageInfo == null)
        {
            return new StagePlayingInfoLoadResponse
            {
                Error = ErrorCode.NotPlayingStage
            };
        }

        return new StagePlayingInfoLoadResponse
        {
            Error = ErrorCode.None,
            Items = redisPlayerStageInfo.FarmingItems, 
            Npcs = redisPlayerStageInfo.Npcs,
            StageId = redisPlayerStageInfo.StageId
        };
    }

    async Task<RedisPlayerStageInfo?> LoadStagePlayerInfo(string userName)
    {
        RedisPlayerStageInfo? redisPlayerStageInfo = await _redisMemoryDB.GetRedisPlayerStageInfo(userName);
        return redisPlayerStageInfo;
    }
}
