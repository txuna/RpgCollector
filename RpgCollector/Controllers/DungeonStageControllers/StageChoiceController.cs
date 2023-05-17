using Microsoft.AspNetCore.Mvc;
using RpgCollector.Models.AccountModel;
using RpgCollector.Models.MasterModel;
using RpgCollector.Models.StageModel;
using RpgCollector.RequestResponseModel;
using RpgCollector.RequestResponseModel.DungeonStageReqRes;
using RpgCollector.Services;
using ZLogger;

namespace RpgCollector.Controllers.DungeonStageControllers;

[ApiController]
public class StageChoiceController : Controller
{
    ILogger<StageChoiceController> _logger;
    IDungeonStageDB _dungeonStageDB;
    IMasterDataDB _masterDataDB;
    IRedisMemoryDB _memoryDB;
    public StageChoiceController(IMasterDataDB masterDataDB, 
                                 IDungeonStageDB dungeonStageDB, 
                                 ILogger<StageChoiceController> logger,
                                 IRedisMemoryDB memoryDB)
    {
        _masterDataDB = masterDataDB;
        _dungeonStageDB = dungeonStageDB;
        _logger = logger;
        _memoryDB = memoryDB;
    }

    [Route("/Stage/Choice")]
    [HttpPost]
    public async Task<StageChoiceResponse> ChoiceStage(StageChoiceRequest stageChoiceRequest)
    {
        int userId = Convert.ToInt32(HttpContext.Items["User-Id"]);
        string authToken = Convert.ToString(HttpContext.Items["Auth-Token"]);
        string userName = stageChoiceRequest.UserName;

        _logger.ZLogDebug($"[{userId}] Request /Stage/Choice");

        if(IsExistStage(stageChoiceRequest.StageId) == false)
        {
            return new StageChoiceResponse
            {
                Error = ErrorCode.NoneExistStageId
            };
        }

        if(await AlreadyEnterStage(userName) == false)
        {
            return new StageChoiceResponse
            {
                Error = ErrorCode.AlreadyPlayStage
            };
        }

        if(await Verify(stageChoiceRequest.StageId, userId) == false)
        {
            return new StageChoiceResponse
            {
                Error = ErrorCode.NeedClearPreconditionStage
            };
        }

        if(await SetPlayerStageInfoInMemory(userName, userId, stageChoiceRequest.StageId) == false)
        {
            return new StageChoiceResponse
            {
                Error = ErrorCode.FailedSetPlayerInfoInRedis
            };
        }

        StageItem[]? stageItem = ProcessingItemResponseValue(stageChoiceRequest.StageId);
        if(stageItem == null)
        {
            return new StageChoiceResponse
            {
                Error = ErrorCode.FaiedLoadStageItem
            };
        }

        StageNpc[]? stageNpc = ProcessingNpcResponseValue(stageChoiceRequest.StageId);
        if(stageNpc == null)
        {
            return new StageChoiceResponse
            {
                Error = ErrorCode.FailedLoadStageNpc
            };
        }

        return new StageChoiceResponse
        {
            Error = ErrorCode.None,
            Items = stageItem,
            Npcs = stageNpc
        };
    }

    StageItem[]? ProcessingItemResponseValue(int stageId)
    {
        MasterStageItem[] masterStageItem = LoadStageFarmingItem(stageId);
        if (masterStageItem == null)
        {
            return null;
        }

        StageItem[] stageItems = new StageItem[masterStageItem.Length];

        for(int i = 0; i < masterStageItem.Length; i++)
        {
            stageItems[i] = new StageItem
            {
                ItemId = masterStageItem[i].ItemId,
                Quantity = masterStageItem[i].Quantity,
            };
        }
        return stageItems; 
    }

    StageNpc[]? ProcessingNpcResponseValue(int stageId)
    {
        MasterStageNpc[] masterStageNpc = LoadStageNpc(stageId);
        if (masterStageNpc == null)
        {
            return null;
        }

        StageNpc[] stageNpcs = new StageNpc[masterStageNpc.Length];

        for(int i = 0; i<masterStageNpc.Length; i++)
        {
            stageNpcs[i] = new StageNpc
            {
                NpcId = masterStageNpc[i].NpcId,
                Count = masterStageNpc[i].Count
            };
        }

        return stageNpcs; 
    }

    async Task<bool> SetPlayerStageInfoInMemory(string userName, int userId, int stageId)
    {
        if (await ChangeUserState(userName, UserState.Playing) == false)
        {
            return false;
        }

        RedisStageItem[] redisStageItem = _masterDataDB.GetRedisStageItems(stageId);
        RedisStageNpc[] redisStageNpc = _masterDataDB.GetRedisStageNpcs(stageId);

        RedisPlayerStageInfo playerStageInfo = new RedisPlayerStageInfo
        {
            UserId = userId, 
            StageId = stageId,
            RewardExp = 0,
            FarmingItems = redisStageItem, 
            Npcs = redisStageNpc,
        };

        if(await _memoryDB.StoreRedisPlayerStageInfo(playerStageInfo, userName) == false)
        {
            if (await ChangeUserState(userName, UserState.Login) == false)
            {
                return false;
            }

            return false;
        }

        return true;
    }

    async Task<bool> ChangeUserState(string userName, UserState userState)
    {
        RedisUser redisUser = (RedisUser)HttpContext.Items["Redis-User"];
        redisUser.State = userState;
        if(await _memoryDB.StoreRedisUser(userName, redisUser) == false)
        {
            return false;
        }

        return true;
    }

    // 던전 플레이 도중 TTL시간이 만료되어 PLAYING이지만 Redis에 플레이정보가 없어졌을 때 PLAYING을 Login으로 변경
    async Task<bool> AlreadyEnterStage(string userName)
    {
        RedisUser user = (RedisUser)HttpContext.Items["Redis-User"]; // 미들웨어에서 이미 NULL 검증 완료

        if (user.State == UserState.Playing)
        {
            RedisPlayerStageInfo? redisPlayerStageInfo = await _memoryDB.GetRedisPlayerStageInfo(userName); 

            if(redisPlayerStageInfo == null)
            {
                if(await ChangeUserState(userName, UserState.Login) == false)
                {
                    return false;
                }

                return true;
            }

            return false;
        }

        return true;
    }

    bool IsExistStage(int stageId)
    {
        return _masterDataDB.IsExistStageId(stageId);
    }

    // 유효한 던전 스테이지인지 확인 필요 
    async Task<bool> Verify(int stageId, int userId)
    {
        PlayerStageInfo? info = await _dungeonStageDB.LoadPlayerStageInfo(userId);
        if (info == null)
        {
            return false;
        }

        if(info.CurStageId < stageId)
        {
            return false;
        }

        return true;
    }

    MasterStageNpc[] LoadStageNpc(int stageId)
    {
        return _masterDataDB.GetMasterStageNpcs(stageId);
    }

    MasterStageItem[] LoadStageFarmingItem(int stageId)
    {
        return _masterDataDB.GetMasterStageItems(stageId);
    }
}
