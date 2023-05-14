using Microsoft.AspNetCore.Mvc;
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
        {
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
        if(await RemovePlayerStageInfo(userName) == false)
        {
            if(await ChangeUserState(userName, authToken, userId, UserState.Playing) == false)
            {
                return new StageClearResponse
                {
                    Error = ErrorCode.CannotChangeUserState
                };
            }
            return new StageClearResponse
            {
                Error = ErrorCode.FailedConnectRedis
            };
        }

        if(await SendStageItemReward(redisPlayerStageInfo) == false)
        {
            return new StageClearResponse
            {
                Error = ErrorCode.FailedSendStageReward
            };
        }

        if(await SetStageExpReward(userId, redisPlayerStageInfo.RewardExp) == false)
        {
            return new StageClearResponse
            {
                Error = ErrorCode.FailedSetStageRewardExp
            };
        }

        // 현재 스테이지 ID와 디비에 저장된 스테이지 ID와 비교하여 같으면 +1 
        if(await SetNextStage(redisPlayerStageInfo.StageId, userId) == false)
        {
            return new StageClearResponse
            {
                Error = ErrorCode.FailedSetNextStage
            };
        }

        return new StageClearResponse
        {
            Error = ErrorCode.None
        };
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
    async Task<bool> SetStageExpReward(int userId, int rewardExp)
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

        int hp = masterPlayerState.First(x => x.Level == level).Hp; 

        if(await _playerAccessDB.UpdatePlayerState(userId, curExp, level, hp) == false)
        {
            return false;
        }

        return true;
    }

    async Task<bool> RemovePlayerStageInfo(string userName)
    {
        if(await _redisMemoryDB.RemoveRedisPlayerStageInfo(userName) == false)
        {
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
