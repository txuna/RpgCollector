using Microsoft.AspNetCore.Mvc;
using RpgCollector.Models.AccountModel;
using RpgCollector.Models.StageModel;
using RpgCollector.RequestResponseModel;
using RpgCollector.RequestResponseModel.DungeonStageReqRes;
using RpgCollector.Services;
using ZLogger;

namespace RpgCollector.Controllers.DungeonStageControllers;

[ApiController]
public class StageHuntingNpcController : Controller
{
    IRedisMemoryDB _redisMemoryDB; 
    ILogger<StageHuntingNpcController> _logger;
    public StageHuntingNpcController(ILogger<StageHuntingNpcController> logger, IRedisMemoryDB redisMemoryDB)
    {
        _logger = logger;
        _redisMemoryDB = redisMemoryDB;
    }

    /**
     *  요건을 충족시켰는지의 검산은 클라이언트에게 
     *  클라이언트가 최종적으로 CLear라는 요청을 보냈을 때 그때 검증
     */
    [Route("/Stage/Hunting/Npc")]
    [HttpPost]
    public async Task<StageHuntingNpcResponse> Post(StageHuntingNpcRequest stageHuntingNpcRequest)
    {
        string userName = Convert.ToString(HttpContext.Items["User-Name"]); 
        int userId = Convert.ToInt32(HttpContext.Items["User-Id"]);

        _logger.ZLogDebug($"[{userId}] Request /Stage/Hunting/Npc");

        RedisPlayerStageInfo? redisPlayerStageInfo = await LoadStagePlayerInfo(userName);
        if(redisPlayerStageInfo == null)
        {
            return new StageHuntingNpcResponse
            {
                Error = ErrorCode.NotPlayingStage
            };
        }

        if(IsExistNpcInStage(redisPlayerStageInfo, stageHuntingNpcRequest.NpcId) == false)
        {
            return new StageHuntingNpcResponse
            {
                Error = ErrorCode.NoneExistNpcInStage
            };
        }

        if(await ProcessingHuntingNpc(userName, redisPlayerStageInfo, stageHuntingNpcRequest.NpcId) == false)
        {
            return new StageHuntingNpcResponse
            {
                Error = ErrorCode.FailedProcessHuntingNpc
            };
        }

        return new StageHuntingNpcResponse
        {
            Error = ErrorCode.None,
            NpcId = stageHuntingNpcRequest.NpcId
        };
    }

    async Task<bool> IsPlayingStage(string userName)
    {
        RedisUser? user = await _redisMemoryDB.GetUser(userName);
        if(user == null)
        {
            return false; 
        }

        if(user.State != UserState.Playing)
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

    bool IsExistNpcInStage(RedisPlayerStageInfo playerInfo, int npcId)
    {
        bool isExist = playerInfo.Npcs.Any(npc => npc.NpcId == npcId);
        return isExist;
    }

    /**
     *  Remaining 갯수 1 줄이고 
     *  RewardExp 늘림
     */
    async Task<bool> ProcessingHuntingNpc(string userName, RedisPlayerStageInfo redisPlayerStageInfo,  int npcId)
    {
        RedisStageNpc? redisStageNpc = redisPlayerStageInfo.Npcs.FirstOrDefault(npc => npc.NpcId == npcId);
        if(redisStageNpc == null)
        {
            return false;
        }

        if(redisStageNpc.RemaingCount > 0)
        {
            redisStageNpc.RemaingCount -= 1;
            redisPlayerStageInfo.RewardExp += redisStageNpc.Exp;
        }

        int index = Array.IndexOf(redisPlayerStageInfo.Npcs, redisStageNpc);
        if(index != -1)
        {
            redisPlayerStageInfo.Npcs[index] = redisStageNpc;
        }

        if(await _redisMemoryDB.StoreRedisPlayerStageInfo(redisPlayerStageInfo, userName) == false)
        {
            return false;
        }

        return true;
    }
}
