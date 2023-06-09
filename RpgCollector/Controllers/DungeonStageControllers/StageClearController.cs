﻿using Microsoft.AspNetCore.Mvc;
using RpgCollector.Models.AccountModel;
using RpgCollector.Models.MasterModel;
using RpgCollector.Models.PackageItemModel;
using RpgCollector.Models.PlayerModel;
using RpgCollector.Models.StageModel;
using RpgCollector.RequestResponseModel;
using RpgCollector.RequestResponseModel.DungeonStageReqRes;
using RpgCollector.Services;
using System.Diagnostics;
using System.Linq.Expressions;
using ZLogger;

namespace RpgCollector.Controllers.DungeonStageControllers;

[ApiController]
public class StageClearController : Controller
{
    IDungeonStageDB _dungeonStageDB;
    IRedisMemoryDB _redisMemoryDB;
    ILogger<StageClearController> _logger;
    IMailboxAccessDB _mailboxAccessDB;
    IMasterDataDB _masterDataDB;
    IPlayerAccessDB _playerAccessDB;
    public StageClearController(IDungeonStageDB dungeonStageDB, 
                                IRedisMemoryDB redisMemoryDB, 
                                ILogger<StageClearController> logger, 
                                IMailboxAccessDB mailboxAccessDB, 
                                IMasterDataDB masterDataDB,
                                IPlayerAccessDB playerAccessDB)
    {
        _dungeonStageDB = dungeonStageDB;
        _redisMemoryDB = redisMemoryDB;
        _logger = logger;
        _mailboxAccessDB = mailboxAccessDB;
        _masterDataDB = masterDataDB;
        _playerAccessDB = playerAccessDB;
    }

    [Route("/Stage/Clear")]
    [HttpPost]
    public async Task<StageClearResponse> Post(StageClearRequest stageClearRequest)
    {
        string userName = Convert.ToString(HttpContext.Items["User-Name"]);
        int userId = Convert.ToInt32(HttpContext.Items["User-Id"]);
        string authToken = Convert.ToString(HttpContext.Items["Auth-Token"]);

        _logger.ZLogDebug($"[{userId}] Request /Stage/Clear");

        if(IsPlayingStage() == false)
        {
            return new StageClearResponse
            {
                Error = ErrorCode.NotPlayingStage
            };
        }

        RedisPlayerStageInfo? redisPlayerStageInfo = await LoadStagePlayerInfo(userName);
        if (redisPlayerStageInfo == null)
        {
            return new StageClearResponse
            {
                Error = ErrorCode.NotPlayingStage
            };
        }

        if (VerifyClearCondition(redisPlayerStageInfo) == false)
        {
            return new StageClearResponse
            {
                Error = ErrorCode.NoneClearStage
            };
        }

        if(await RemovePlayerStageInfoInMemory(userName) == false)
        {
            return new StageClearResponse
            {
                Error = ErrorCode.FailedRemoveStageInfoInMemory
            };
        }

        if(await TakeStageReward(userId, redisPlayerStageInfo, stageClearRequest.PlayerHp) == false)
        {
            return new StageClearResponse
            {
                Error = ErrorCode.FailedSendStageReward
            };
        }

        return new StageClearResponse
        {
            Error = ErrorCode.None
        };
    }

    async Task<bool> TakeStageReward(int userId, RedisPlayerStageInfo redisPlayerStageInfo, int hp)
    {
        if (await SendStageItemReward(redisPlayerStageInfo) == false)
        {
            return false;
        }

        if(await SetStageExpReward(userId, redisPlayerStageInfo.RewardExp, hp) == false)
        {
            return false; 
        }

        if(await SetNextStage(redisPlayerStageInfo.StageId, userId) == false)
        {
            return false;
        }

        return true;
    }

    // farming item 메일로 전송 및 경험치 설정
    async Task<bool> SendStageItemReward(RedisPlayerStageInfo redisPlayerStageInfo)
    {
        object[][] values = new object[redisPlayerStageInfo.FarmingItems.Length][];

        int index = 0;
        foreach (RedisStageItem item in redisPlayerStageInfo.FarmingItems)
        {
            if(item.FarmingCount == 0)
            {
                continue;
            }
            values[index] = new object[] { 1,
                                           redisPlayerStageInfo.UserId,
                                           $"Stage {redisPlayerStageInfo.StageId} Clear Reward!",
                                           "Congratulations on clearing the dungeon! Here's your well-deserved reward!",
                                            0, 0, item.ItemId, item.FarmingCount, 0, DateTime.Now.AddDays(30)
            };
            index += 1;
        }

        if (await _mailboxAccessDB.SendMultipleMail(values) == false)
        {
            return false;
        }

        return true;
    }

    // 플레이어의 현재 스테이지와 같으면 증가 그렇지 않다면 패스
    async Task<bool> SetNextStage(int stageId, int userId)
    {
        var (Error, result) = await _dungeonStageDB.SetNextStage(userId, stageId);

        if (Error != ErrorCode.None)
        {
            return false;
        }

        return true;
    }
    
    // 경험치 초과시 레벨 설정 
    async Task<bool> SetStageExpReward(int userId, int rewardExp, int hp)
    {
        MasterPlayerState[]? masterPlayerState = _masterDataDB.GetMasterAllPlayerState();
        if(masterPlayerState == null)
        {
            return false;
        }

        PlayerState? playerState = await _playerAccessDB.GetPlayerFromUserId(userId);
        if(playerState == null)
        {
            return false;
        }

        int curExp = playerState.Exp + rewardExp;
        int level = playerState.Level;
        int max = masterPlayerState.First(x => x.Level == level).Exp;
        
        while (curExp >= max)
        {
            level += 1;
            curExp -= max; 
            max = masterPlayerState.First(x => x.Level == level).Exp;
        }

        int maxHp = masterPlayerState.First(x => x.Level == level).Hp;
        if(hp <= 0 || hp > maxHp)
        {
            hp = 1;
        }

        if (await _playerAccessDB.UpdatePlayerState(userId, curExp, level, hp) == false)
        {
            return false;
        }

        return true;
    }

    async Task<bool> RemovePlayerStageInfoInMemory(string userName)
    {
        if(await ChangeUserState(userName, UserState.Login) == false)
        {
            return false;
        }

        if(await _redisMemoryDB.RemoveRedisPlayerStageInfo(userName) == false)
        {
            if (await ChangeUserState(userName, UserState.Playing) == false)
            {
                return false;
            }

            return false;
        }
        return true;
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

    bool IsPlayingStage()
    {
        RedisUser user = (RedisUser)HttpContext.Items["Redis-User"];

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
}
