using Microsoft.AspNetCore.Mvc;
using RpgCollector.Models.AccountModel;
using RpgCollector.Models.StageModel;
using RpgCollector.RequestResponseModel;
using RpgCollector.RequestResponseModel.DungeonStageReqRes;
using RpgCollector.Services;
using System.Diagnostics;
using System.Linq.Expressions;

namespace RpgCollector.Controllers.DungeonStageControllers;

[ApiController]
public class StageClearController : Controller
{
    IDungeonStageDB _dungeonStageDB;
    IRedisMemoryDB _redisMemoryDB;
    ILogger<StageClearController> _logger;
    public StageClearController(IDungeonStageDB dungeonStageDB, IRedisMemoryDB redisMemoryDB, ILogger<StageClearController> logger)
    {
        _dungeonStageDB = dungeonStageDB;
        _redisMemoryDB = redisMemoryDB;
        _logger = logger;
    }

    [Route("/Stage/Clear")]
    [HttpPost]
    public async Task<StageClearResponse> Post(StageClearRequest stageClearRequest)
    {
        string userName = Convert.ToString(HttpContext.Items["User-Name"]);
        int userId = Convert.ToInt32(HttpContext.Items["User-Id"]);
        string authToken = Convert.ToString(HttpContext.Items["Auth-Token"]);

        // 현재 던전 중인지 확인
        if (await IsPlayingStage(userName) == false)
        {
            return new StageClearResponse
            {
                Error = ErrorCode.NotPlayingStage
            };
        }

        // 클리어 검증 
        RedisPlayerStageInfo? redisPlayerStageInfo = await LoadStagePlayerInfo(userName);
        if (redisPlayerStageInfo == null)
        {
            return new StageClearResponse
            {
                Error = ErrorCode.NotPlayingStage
            };
        }

        if (VerifyClearCondition(redisPlayerStageInfo) == false)
        {)
            return new StageClearResponse
            {
                Error = ErrorCode.NoneClearStage
            };
        }

        // 유저 상태 변경 
        if (await ChangeUserState(userName, authToken, userId, UserState.Login) == false)
        {
            return new StageClearResponse
            {
                Error = ErrorCode.RedisErrorCannotEnterStage
            };
        }

        // 던전 내용 백업 및 삭제 - 추후 아래 내용 실패시 user - playing으로 바꾸고 던전 내용 다시 삽입

        // 경험치 보상 및 파밍 아이템 제공

        // 현재 스테이지 ID와 디비에 저장된 스테이지 ID와 비교하여 같으면 +1 

        return new StageClearResponse
        {
            Error = ErrorCode.None
        };
    }

    bool VerifyClearCondition(RedisPlayerStageInfo redisPlayerStageInfo)
    {
        // NPC 다 잡았는지 확인
        foreach(var npc in redisPlayerStageInfo.Npcs)
        {
            if(npc.RemaingCount != 0)
            {
                return false;
            }
        }

        return true;
    }

    async Task<bool> IsPlayingStage(string userName)
    {
        RedisUser? user = await _redisMemoryDB.GetUser(userName);
        if (user == null)
        {
            return false;
        }

        if (user.State != UserState.Playing)
        {
            return false;
        }

        return true;
    }

    async Task<RedisPlayerStageInfo?> LoadStagePlayerInfo(string userName)
    {
        RedisPlayerStageInfo? redisPlayerStageInfo = await _redisMemoryDB.GetRedisPlayerStageInfo(userName);
        return redisPlayerStageInfo;
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
