using Microsoft.AspNetCore.Mvc;
using RpgCollector.Models.AccountModel;
using RpgCollector.Models.StageModel;
using RpgCollector.RequestResponseModel;
using RpgCollector.RequestResponseModel.DungeonStageReqRes;
using RpgCollector.Services;

namespace RpgCollector.Controllers.DungeonStageControllers;

[ApiController]
public class StageFarmingItemController : Controller
{
    IDungeonStageDB _dungeonStageDB;
    IRedisMemoryDB _redisMemoryDB;
    ILogger<StageFarmingItemController> _logger;
    public StageFarmingItemController(IDungeonStageDB dungeonStageDB, IRedisMemoryDB redisMemoryDB, ILogger<StageFarmingItemController> logger)
    {
        _dungeonStageDB = dungeonStageDB;
        _redisMemoryDB = redisMemoryDB;
        _logger = logger;
    }

    /**
     * 해당 아이템이 스테이지에서 나올 수 있는건지 검증 필요
     */
    [Route("/Stage/Farming/Item")]
    [HttpPost]
    public async Task<StageFarmingItemResponse> Post(StageFarmingItemRequest stageFarmingItemRequest)
    {
        string userName = Convert.ToString(HttpContext.Items["User-Name"]);
        int userId = Convert.ToInt32(HttpContext.Items["User-Id"]);

        RedisPlayerStageInfo? redisPlayerStageInfo = await LoadStagePlayerInfo(userName);
        if (redisPlayerStageInfo == null)
        {
            return new StageFarmingItemResponse
            {
                Error = ErrorCode.NotPlayingStage
            };
        }

        if (IsExistItemInStage(redisPlayerStageInfo, stageFarmingItemRequest.ItemId) == false)
        {
            return new StageFarmingItemResponse
            {
                Error = ErrorCode.NoneExistNpcInStage
            };
        }

        if(await ProcessingFarmingItem(userName, redisPlayerStageInfo, stageFarmingItemRequest.ItemId) == false)
        {
            return new StageFarmingItemResponse
            {
                Error = ErrorCode.FailedProcessFarmingItem
            };
        }

        return new StageFarmingItemResponse
        {
            Error = ErrorCode.None,
            ItemId = stageFarmingItemRequest.ItemId
        };
    }

    // 아이템 갯수 초과 확인
    async Task<bool> ProcessingFarmingItem(string userName, RedisPlayerStageInfo redisPlayerStageInfo, int itemId)
    {
        RedisStageItem? redisStageItem = redisPlayerStageInfo.FarmingItems.FirstOrDefault(item => item.ItemId == itemId);
        if(redisStageItem == null)
        {
            return false;
        }

        if(redisStageItem.FarmingCount >= redisStageItem.MaxCount)
        {
            return false;
        }

        redisStageItem.FarmingCount += 1;

        int index = Array.IndexOf(redisPlayerStageInfo.Npcs, redisStageItem);
        if (index != -1)
        {
            redisPlayerStageInfo.FarmingItems[index] = redisStageItem;
        }

        if (await _redisMemoryDB.StoreRedisPlayerStageInfo(redisPlayerStageInfo, userName) == false)
        {
            return false;
        }

        return true;
    }

    bool IsExistItemInStage(RedisPlayerStageInfo playerInfo, int itemId)
    {
        bool isExist = playerInfo.FarmingItems.Any(item => item.ItemId == itemId);
        return isExist;
    }

    async Task<RedisPlayerStageInfo?> LoadStagePlayerInfo(string userName)
    {
        RedisPlayerStageInfo? redisPlayerStageInfo = await _redisMemoryDB.GetRedisPlayerStageInfo(userName);
        return redisPlayerStageInfo;
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

}
