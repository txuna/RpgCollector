using Microsoft.AspNetCore.Mvc;
using RpgCollector.Models.AccountModel;
using RpgCollector.RequestResponseModel;
using RpgCollector.RequestResponseModel.DungeonStageReqRes;
using RpgCollector.Services;
using ZLogger;

namespace RpgCollector.Controllers.DungeonStageControllers;

[ApiController]
public class StageExitController : Controller
{
    IRedisMemoryDB _redisMemoryDB;
    ILogger<StageExitController> _logger;
    public StageExitController(IRedisMemoryDB redisMemoryDB, ILogger<StageExitController> logger)
    {
        _redisMemoryDB = redisMemoryDB;
        _logger = logger;
    }

    [Route("/Stage/Exit")]
    [HttpPost]
    public async Task<StageExitResponse> Post(StageExitRequest stageExistRequest)
    {
        string userName = Convert.ToString(HttpContext.Items["User-Name"]);
        int userId = Convert.ToInt32(HttpContext.Items["User-Id"]);
        string authToken = Convert.ToString(HttpContext.Items["Auth-Token"]);

        _logger.ZLogDebug($"[{userId}] Request /Stage/Clear");

        if(IsPlayingStage() == false)
        {
            return new StageExitResponse
            {
                Error = ErrorCode.NotPlayingStage
            };
        }

        if (await RemovePlayerStageInfoInMemory(userName) == false)
        {
            return new StageExitResponse
            {
                Error = ErrorCode.FailedRemoveStageInfoInMemory
            };
        }

        return new StageExitResponse
        {
            Error = ErrorCode.None
        };
    }

    bool IsPlayingStage()
    {
        RedisUser user = (RedisUser)HttpContext.Items["Redis-User"];

        if (user.State != UserState.Playing)
        {
            return false;
        }

        return true;
    }

    async Task<bool> ChangeUserState(string userName, UserState userState)
    {
        RedisUser redisUser = (RedisUser)HttpContext.Items["Redis-User"];
        redisUser.State = userState;
        if (await _redisMemoryDB.StoreRedisUser(userName, redisUser) == false)
        {
            return false;
        }

        return true;
    }

    async Task<bool> RemovePlayerStageInfoInMemory(string userName)
    {
        if (await ChangeUserState(userName,UserState.Login) == false)
        {
            return false;
        }

        if (await _redisMemoryDB.RemoveRedisPlayerStageInfo(userName) == false)
        {
            if (await ChangeUserState(userName, UserState.Playing) == false)
            {
                return false;
            }

            return false;
        }
        return true;
    }
}
