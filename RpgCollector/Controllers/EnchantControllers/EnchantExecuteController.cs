using Microsoft.AspNetCore.Mvc;
using RpgCollector.RequestResponseModel.EnchantModel;
using RpgCollector.RequestResponseModel;
using RpgCollector.Services;
using RpgCollector.Models.MasterModel;
using RpgCollector.Models.EnchantModel;
using ZLogger;
using RpgCollector.Models.PlayerModel;
using RpgCollector.Models;
using RpgCollector.Models.AccountModel;
using StackExchange.Redis;

namespace RpgCollector.Controllers.EnchantControllers;

[ApiController]
public class EnchantExecuteController : Controller
{
    IEnchantDB _enchantDB;  
    IAccountMemoryDB _accountMemoryDB;
    ILogger<EnchantExecuteController> _logger;
    IPlayerAccessDB _playerAccessDB;
    IMasterDataDB _masterDataDB;

    public EnchantExecuteController(IEnchantDB enchantDB, 
                                    IAccountMemoryDB accountMemoryDB,
                                    IPlayerAccessDB playerAccessDB, 
                                    ILogger<EnchantExecuteController> logger,
                                    IMasterDataDB masterDataDB)
    {
        _enchantDB = enchantDB;
        _accountMemoryDB = accountMemoryDB;
        _playerAccessDB = playerAccessDB;
        _masterDataDB = masterDataDB;
        _logger = logger;
    }

    /*
    1. TypeDefinition 확인 (장비아이템만) 
    2. EchantCount와 MaxEnchatCount 비교 
    3.다음번에 갈 강화테이블 참조 
    3.1 강화 참조 테이블 
    EnchantCountId - Percent - increasementValue
    4. 실패시 해당 아이템 삭제 
    5. 강화 이력 테이블 갱신 
    logId - userId - playerItemid - isSuccess - date 
    실제 전투시 강화 테이블을 참조하여 데미지 계산 - 4성이면 프로토타입에서 10% 4번 곱하기
     */
    [Route("/Enchant")]
    [HttpPost]
    public async Task<EnchantExecuteResponse> Enchant(EnchantExecuteRequest enchantExecuteRequest)
    {
        int playerItemId = enchantExecuteRequest.PlayerItemId;
        ErrorState Error;
        int result;

        string userName = HttpContext.Request.Headers["User-Name"];
        int userId = await _accountMemoryDB.GetUserId(userName);

        _logger.ZLogInformation($"[{userId} {userName}] Request 'Enchant'");

        /* 플레이어의 아이템 로드 */
        PlayerItem? playerItem = await _playerAccessDB.GetPlayerItem(playerItemId);

        if (playerItem == null)
        {
            return new EnchantExecuteResponse
            {
                Error = ErrorState.NoneExistItem
            };
        }
        /* ItemId를 기반으로 마스터 아이템 로드 */
        MasterItem? masterItem = _masterDataDB.GetMasterItem(playerItem.ItemId);

        if (masterItem == null)
        {
            return new EnchantExecuteResponse
            {
                Error = ErrorState.NoneExistItem
            };
        }

        /* 강화 유효성 판단 */
        Error = await Verify(playerItem, masterItem, userId); 
        if(Error != ErrorState.None)
        {
            return new EnchantExecuteResponse
            {
                Error = Error
            };
        }

        /* 아이템 강화 진행 */
        (Error, result) = await ExecuteEnchant(playerItem, userId);
        
        return new EnchantExecuteResponse
        {
            Error = Error,
            Result = result
        };
    }

    async Task<ErrorState> Verify(PlayerItem playerItem, MasterItem masterItem, int userId)
    {
        ErrorState Error;

        Error = await VerifyItemPermission(playerItem.PlayerItemId, userId);
        if(Error != ErrorState.None)
        {
            return Error;
        }

        Error = VerifyItemType(masterItem.AttributeId);
        if (Error != ErrorState.None)
        {
            return Error;
        }

        Error = VerifyEnchatMaxCount(playerItem, masterItem);
        if (Error != ErrorState.None)
        {
            return Error;
        }

        Error = await VerifyMoney(userId, playerItem.EnchantCount + 1);
        if (Error != ErrorState.None)
        {
            return Error;
        }

        return ErrorState.None;
    }

    async Task<ErrorState> VerifyMoney(int userId, int nextEnchantCount)
    {
        MasterEnchantInfo masterEnchantInfo = _masterDataDB.GetMasterEnchantInfo(nextEnchantCount);
        int playerMoney = await _playerAccessDB.GetPlayerMoney(userId);

        if (playerMoney < masterEnchantInfo.Price)
        {
            return ErrorState.NotEnoughMoney;
        }

        return ErrorState.None;
    }

    async Task<ErrorState> VerifyItemPermission(int playerItemId, int userId)
    {
        if (!await _playerAccessDB.IsItemOwner(playerItemId, userId))
        {
            return ErrorState.IsNotOwnerThisItem;
        }

        return ErrorState.None;
    }

    ErrorState VerifyItemType(int attributeId)
    {
        MasterItemAttribute? itemAttribute = _masterDataDB.GetMasterItemAttribute(attributeId);

        if (itemAttribute == null)
        {
            return ErrorState.NoneExistItemType;
        }
        if (itemAttribute.TypeId != (int)TypeDefinition.EQUIPMENT)
        {
            return ErrorState.CantNotEnchantThisType;
        }

        return ErrorState.None;
    }

    ErrorState VerifyEnchatMaxCount(PlayerItem playerItem, MasterItem masterItem)
    {
        if (playerItem.EnchantCount >= masterItem.MaxEnchantCount)
        {
            return ErrorState.AlreadyMaxiumEnchantCount;
        }
        return ErrorState.None;
    }

    (ErrorState, int) CheckPercent(PlayerItem playerItem, int userId)
    {
        // 현재 강화 진행 상태에 따른 강화확률에 따라강화 진행 
        MasterEnchantInfo? masterEnchantInfo = _masterDataDB.GetMasterEnchantInfo(playerItem.EnchantCount + 1);

        if (masterEnchantInfo == null)
        {
            return (ErrorState.NoneExistEnchantCount, -1);
        }

        Random random = new Random();
        int randomValue = random.Next(101);
        int result = randomValue < masterEnchantInfo.Percent ? 1 : 0;

        return (ErrorState.None, result);
    }

    /*
     확률을 참고하며 실패시 아이템 삭제
     */
    async Task<(ErrorState, int)> ExecuteEnchant(PlayerItem playerItem, int userId)
    {
        var (Error, result) = CheckPercent(playerItem, userId);
        if (Error != ErrorState.None)
        {
            return (Error, -1);
        }

        if (result == 1)
        {
            if (!await _enchantDB.DoEnchant(playerItem))
            {
                return (ErrorState.NoneExistItem, -1);
            }
        }
        else
        {
            if (!await _playerAccessDB.RemovePlayerItem(playerItem.PlayerItemId))
            {
                return (ErrorState.NoneExistItem, -1);
            }
        }

        MasterEnchantInfo masterEnchantInfo = _masterDataDB.GetMasterEnchantInfo(playerItem.EnchantCount + 1);

        await _playerAccessDB.AddMoneyToPlayer(userId, -masterEnchantInfo.Price);

        return await LogEnchant(playerItem, userId, result);
    }

    async Task<(ErrorState, int)> LogEnchant(PlayerItem playerItem, int userId, int result)
    {
        if (!await _enchantDB.EnchantLog(playerItem.PlayerItemId, userId, playerItem.EnchantCount, result))
        {
            _logger.ZLogError($"[{userId}] Failed Enchant Logging Player Item : {playerItem.PlayerItemId}");
        }

       return (ErrorState.None, result);
    }
}