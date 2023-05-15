﻿using Microsoft.AspNetCore.Mvc;
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
        //TODO:최흥배. 코드를 좀 나누어주세요. 보기가 좋지는 않네요

        int userId = Convert.ToInt32(HttpContext.Items["User-Id"]);
        string authToken = Convert.ToString(HttpContext.Items["Auth-Token"]);
        //TODO:최흥배. 유저이름은 클라이언트로 부터 요청 받으면 안됩니다. 신뢰하기가 어렵습니다.
        // 그리고 이것을 받는 이유도 불필요합니다. userId를 Redis의 key로 사용하는게 더 좋습니다.
        string userName = stageChoiceRequest.UserName;

        _logger.ZLogDebug($"[{userId}] Request /Stage/Choice");

        if(await AlreadyEnterStage(userName, authToken, userId) == false)
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

        MasterStageItem[] masterStageItem = LoadStageFarmingItem(stageChoiceRequest.StageId);
        if(masterStageItem == null)
        {
            return new StageChoiceResponse
            {
                Error = ErrorCode.FaiedLoadStageItem
            };
        }

        MasterStageNpc[] masterStageNpc = LoadStageNpc(stageChoiceRequest.StageId);
        if(masterStageNpc == null)
        {
            return new StageChoiceResponse
            {
                Error = ErrorCode.FailedLoadStageNpc
            };
        }

        if(await ChangeUserState(userName, authToken, userId, UserState.Playing) == false)
        {
            return new StageChoiceResponse
            {
                Error = ErrorCode.RedisErrorCannotEnterStage
            };
        }

        if(await SetPlayerStageInfoInMemory(userName, userId, stageChoiceRequest.StageId, masterStageItem, masterStageNpc) == false)
        {
            //TODO: 최흥배. 처음부터 RedisUser를 다 미들웨어에서 가져왔으면 더 좋았을 것 가텐요. 던전 플레이로 상태만 바꾸는 것인데 아래 함수는 모든 정보를 다 바꾸는 것 같습니다.
            if (await ChangeUserState(userName, authToken, userId, UserState.Login) == false)
            {
                return new StageChoiceResponse
                {
                    Error = ErrorCode.CannotChangeUserState
                };
            }
            return new StageChoiceResponse
            {
                Error = ErrorCode.RedisErrorCannotEnterStage
            };
        }

        return new StageChoiceResponse
        {
            Error = ErrorCode.None,
            Items = ProcessingItemResponseValue(masterStageItem),
            Npcs = ProcessingNpcResponseValue(masterStageNpc)
        };
    }

    StageItem[] ProcessingItemResponseValue(MasterStageItem[] masterStageItem)
    {
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

    StageNpc[] ProcessingNpcResponseValue(MasterStageNpc[] masterStageNpc)
    {
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

    async Task<bool> SetPlayerStageInfoInMemory(string userName, 
                                                int userId, 
                                                int stageId, 
                                                MasterStageItem[] masterStageItem, 
                                                MasterStageNpc[] masterStageNpc)
    {
        //TODO:최흥배. 마스터데이터에서 처음부터 Redis에 담을 형태로 가져왔으면 되는데 여기서 불필요하게 정리하는 것 같네요.
        RedisStageItem[] redisStageItem = new RedisStageItem[masterStageItem.Length];
        RedisStageNpc[] redisStageNpc = new RedisStageNpc[masterStageNpc.Length]; 

        for(int i=0; i<masterStageItem.Length; i++)
        {
            redisStageItem[i] = new RedisStageItem
            {
                ItemId = masterStageItem[i].ItemId,
                MaxCount = masterStageItem[i].Quantity, 
                FarmingCount = 0
            };
        }

        for(int i=0; i<masterStageNpc.Length; i++)
        {
            redisStageNpc[i] = new RedisStageNpc
            {
                NpcId = masterStageNpc[i].NpcId,
                Count = masterStageNpc[i].Count,
                RemaingCount = masterStageNpc[i].Count,
                Exp = masterStageNpc[i].Exp
            };
        }

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
            return false;
        }

        return true;
    }

    async Task<bool> ChangeUserState(string userName, string authToken, int userId, UserState userState)
    {
        if(await _memoryDB.StoreUser(userName, userId, authToken, userState) == false)
        {
            return false;
        }

        return true;
    }

    // 던전 플레이 도중 TTL시간이 만료되어 PLAYING이지만 Redis에 플레이정보가 없어졌을 때 PLAYING을 Login으로 변경
    async Task<bool> AlreadyEnterStage(string userName, string authToken, int userId)
    {
        //TODO:최흥배. 여기에 게임 플레이 상태를 넣을 것이라면 미들웨어에서 RedisUser 전체를 컨트룰러로 다 넘겨주세요. redis 접근 횟수를 줄이는게 좋겠죠
        RedisUser? user = await _memoryDB.GetUser(userName);
        if(user == null)
        {
            return false; 
        }

        //TODO: 최흥배. 이미 플레이 중이라면 연결하서 하던가 또는 기존 데이터를 다 지워야 하는데 그렇게 하는 것일까요?
        if (user.State == UserState.Playing)
        {
            RedisPlayerStageInfo? redisPlayerStageInfo = await _memoryDB.GetRedisPlayerStageInfo(userName); 
            if(redisPlayerStageInfo == null)
            {
                if(await ChangeUserState(userName, authToken, userId, UserState.Login) == false)
                {
                    return false;
                }
                return true;
            }
            return false;
        }

        return true;
    }

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
