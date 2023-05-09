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
    ILogger<EnchantExecuteController> _logger;
    IPlayerAccessDB _playerAccessDB;
    IMasterDataDB _masterDataDB;

    public EnchantExecuteController(IEnchantDB enchantDB, 
                                    IPlayerAccessDB playerAccessDB, 
                                    ILogger<EnchantExecuteController> logger,
                                    IMasterDataDB masterDataDB)
    {
        _enchantDB = enchantDB;
        _playerAccessDB = playerAccessDB;
        _masterDataDB = masterDataDB;
        _logger = logger;
    }

    [Route("/Enchant")]
    [HttpPost]
    public async Task<EnchantExecuteResponse> Enchant(EnchantExecuteRequest enchantExecuteRequest)
    {
        int playerItemId = enchantExecuteRequest.PlayerItemId;
        ErrorState Error;
        int result;

        int userId = Convert.ToInt32(HttpContext.Items["User-Id"]);

        _logger.ZLogInformation($"[{userId}] Request /Enchant");

        PlayerItem? playerItem = await _playerAccessDB.GetPlayerItem(playerItemId);

        if (playerItem == null)
        {
            return new EnchantExecuteResponse
            {
                Error = ErrorState.NoneExistItem
            };
        }

        MasterItem? masterItem = _masterDataDB.GetMasterItem(playerItem.ItemId);

        if (masterItem == null)
        {
            return new EnchantExecuteResponse
            {
                Error = ErrorState.NoneExistItem
            };
        }

        Error = await Verify(playerItem, masterItem, userId); 

        if(Error != ErrorState.None)
        {
            return new EnchantExecuteResponse
            {
                Error = Error
            };
        }

        (Error, result) = await ExecuteEnchant(playerItem, userId);

        if(Error != ErrorState.None)
        {
            return new EnchantExecuteResponse
            {
                Error = Error
            };
        }

        MasterEnchantInfo masterEnchantInfo = _masterDataDB.GetMasterEnchantInfo(playerItem.EnchantCount + 1);

        if(!await _playerAccessDB.AddMoneyToPlayer(userId, -masterEnchantInfo.Price))
        {
            return new EnchantExecuteResponse
            {
                Error = ErrorState.FailedFetchMoney
            };
        }

        await LogEnchant(playerItem, userId, result);
        
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

        return (ErrorState.None, result);
    }

    async Task<ErrorState> LogEnchant(PlayerItem playerItem, int userId, int result)
    {
        if (!await _enchantDB.EnchantLog(playerItem.PlayerItemId, userId, playerItem.EnchantCount, result))
        {
            _logger.ZLogError($"[{userId}] Failed Enchant Logging Player Item : {playerItem.PlayerItemId}");
        }

       return ErrorState.None;
    }
}