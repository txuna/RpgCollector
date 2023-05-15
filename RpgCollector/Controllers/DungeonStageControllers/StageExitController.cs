using Microsoft.AspNetCore.Mvc;
using RpgCollector.Models.AccountModel;
using RpgCollector.RequestResponseModel.DungeonStageReqRes;
using RpgCollector.Services;
using ZLogger;

namespace RpgCollector.Controllers.DungeonStageControllers
{
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
                    Error = RequestResponseModel.ErrorCode.NotPlayingStage
                };
            }

            if(await ChangeUserState(userName, authToken, userId, UserState.Login) == false)
            {
                return new StageExitResponse
                {
                    Error = RequestResponseModel.ErrorCode.CannotChangeUserState
                };
            }

            if(await RemoveStageInfoInMemory(userName) == false)
            {
                if(await ChangeUserState(userName, authToken, userId, UserState.Playing) == false)
                {
                    return new StageExitResponse
                    {
                        Error = RequestResponseModel.ErrorCode.CannotChangeUserState
                    };
                }
                return new StageExitResponse
                {
                    Error = RequestResponseModel.ErrorCode.FailedRemoveStageInfoInMemory
                };
            }

            return new StageExitResponse
            {
                Error = RequestResponseModel.ErrorCode.None
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

        async Task<bool> RemoveStageInfoInMemory(string userName)
        {
            if(await _redisMemoryDB.RemoveRedisPlayerStageInfo(userName) == false)
            {
                return false;
            }

            return true;
        }

        async Task<bool> ChangeUserState(string userName, string authToken, int userId, UserState userState)
        {
            if (await _redisMemoryDB.StoreUser(userName, userId, authToken, userState) == false)
            {
                return false;
            }

            return true;
        }
    }
}
