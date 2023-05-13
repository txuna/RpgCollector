﻿using Microsoft.AspNetCore.Mvc;
using RpgCollector.Models.AccountModel;
using RpgCollector.Models.MasterModel;
using RpgCollector.Models.StageModel;
using RpgCollector.RequestResponseModel;
using RpgCollector.RequestResponseModel.DungeonStageReqRes;
using RpgCollector.Services;

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

        MasterStageItem[] masterStageItem = LoadStageItem(stageChoiceRequest.StageId);
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

        // 실패시 다시 Login 상태로 
        if(await SetPlayerStageInfoInMemory(userName, userId, stageChoiceRequest.StageId, masterStageItem, masterStageNpc) == false)
        {
            if(await ChangeUserState(userName, authToken, userId, UserState.Login) == false)
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
                ItemId = masterStageItem[i].ItemId
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
        RedisStageItem[] redisStageItem = new RedisStageItem[masterStageItem.Length];
        RedisStageNpc[] redisStageNpc = new RedisStageNpc[masterStageNpc.Length]; 

        for(int i=0; i<masterStageItem.Length; i++)
        {
            redisStageItem[i] = new RedisStageItem
            {
                ItemId = masterStageItem[i].ItemId,
                IsFarming = false
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

    async Task<bool> AlreadyEnterStage(string userName)
    {
        RedisUser? user = await _memoryDB.GetUser(userName);
        if(user == null)
        {
            return false; 
        }

        if(user.State == UserState.Playing)
        {
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

    MasterStageItem[] LoadStageItem(int stageId)
    {
        return _masterDataDB.GetMasterStageItems(stageId);
    }
}
