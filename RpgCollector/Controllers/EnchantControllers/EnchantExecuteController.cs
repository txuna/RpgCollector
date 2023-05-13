using Microsoft.AspNetCore.Mvc;
using RpgCollector.RequestResponseModel;
using RpgCollector.Services;
using RpgCollector.Models.MasterModel;
using ZLogger;
using RpgCollector.Models.PlayerModel;
using RpgCollector.Models;
using RpgCollector.Models.AccountModel;
using StackExchange.Redis;
using RpgCollector.RequestResponseModel.EnchantReqRes;

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
        ErrorCode Error;
        int result;

        int userId = Convert.ToInt32(HttpContext.Items["User-Id"]);

        PlayerItem? playerItem = await _playerAccessDB.GetPlayerItem(playerItemId, userId);

        if (playerItem == null)
        {
            return new EnchantExecuteResponse
            {
                Error = ErrorCode.FailedFetchPlayerItem
            };
        }

        MasterItem? masterItem = _masterDataDB.GetMasterItem(playerItem.ItemId);

        if (masterItem == null)
        {
            return new EnchantExecuteResponse
            {
                Error = ErrorCode.NoneExistItem
            };
        }

        MasterEnchantInfo masterEnchantInfo = _masterDataDB.GetMasterEnchantInfo(playerItem.EnchantCount + 1);

        Error = await Verify(playerItem, masterItem, userId, masterEnchantInfo); 

        if(Error != ErrorCode.None)
        {
            return new EnchantExecuteResponse
            {
                Error = Error
            };
        }

        (Error, result) = await ExecuteEnchant(playerItem, masterItem, masterEnchantInfo);

        if(Error != ErrorCode.None)
        {
            return new EnchantExecuteResponse
            {
                Error = Error
            };
        }

        if (!await _playerAccessDB.SubtractionMoneyToPlayer(userId, masterEnchantInfo.Price))
        {
            return new EnchantExecuteResponse
            {
                Error = ErrorCode.FailedFetchMoney
            };
        }
        
        return new EnchantExecuteResponse
        {
            Error = Error,
            Result = result
        };
    }

    async Task<ErrorCode> Verify(PlayerItem playerItem, MasterItem masterItem, int userId, MasterEnchantInfo masterEnchantInfo)
    {
        ErrorCode Error;

        Error = VerifyItemType(masterItem.AttributeId);

        if (Error != ErrorCode.None)
        {
            return Error;
        }

        Error = VerifyEnchatMaxCount(playerItem, masterItem);

        if (Error != ErrorCode.None)
        {
            return Error;
        }

        Error = await VerifyMoney(userId, masterEnchantInfo);

        if (Error != ErrorCode.None)
        {
            return Error;
        }

        return ErrorCode.None;
    }

    async Task<ErrorCode> VerifyMoney(int userId, MasterEnchantInfo masterEnchantInfo)
    {
        int playerMoney = await _playerAccessDB.GetPlayerMoney(userId);

        if (playerMoney < masterEnchantInfo.Price)
        {
            return ErrorCode.NotEnoughMoney;
        }

        return ErrorCode.None;
    }

    ErrorCode VerifyItemType(int attributeId)
    {
        MasterItemAttribute? itemAttribute = _masterDataDB.GetMasterItemAttribute(attributeId);

        if (itemAttribute == null)
        {
            return ErrorCode.NoneExistItemType;
        }

        if (itemAttribute.TypeId != (int)TypeDefinition.EQUIPMENT)
        {
            return ErrorCode.CantNotEnchantThisType;
        }

        return ErrorCode.None;
    }

    ErrorCode VerifyEnchatMaxCount(PlayerItem playerItem, MasterItem masterItem)
    {
        if (playerItem.EnchantCount >= masterItem.MaxEnchantCount)
        {
            return ErrorCode.AlreadyMaxiumEnchantCount;
        }

        return ErrorCode.None;
    }

    (ErrorCode, int) CalculateCanEnchant(MasterEnchantInfo masterEnchantInfo)
    {
        Random random = new Random();
        int randomValue = random.Next(101);
        int result = randomValue < masterEnchantInfo.Percent ? 1 : 0;

        return (ErrorCode.None, result);
    }

    async Task<(ErrorCode, int)> ExecuteEnchant(PlayerItem playerItem, MasterItem masterItem, MasterEnchantInfo masterEnchantInfo)
    {
        var (Error, result) = CalculateCanEnchant(masterEnchantInfo);

        if (Error != ErrorCode.None)
        {
            return (Error, -1);
        }

        if (result == 1)
        {
            playerItem = CalculateIncreasementStatsValue(playerItem, masterItem, masterEnchantInfo);

            if (await _enchantDB.DoEnchant(playerItem) == false)
            {
                return (ErrorCode.NoneExistItem, -1);
            }
        }
        else
        {
            if (await _playerAccessDB.RemovePlayerItem(playerItem.PlayerItemId) == false)
            {
                return (ErrorCode.NoneExistItem, -1);
            }
        }

        return (ErrorCode.None, result);
    }

    PlayerItem CalculateIncreasementStatsValue(PlayerItem playerItem, MasterItem masterItem, MasterEnchantInfo masterEnchantInfo)
    {
        if(masterItem.AttributeId == 1)
        {
            playerItem.Attack += (int)Math.Ceiling((double)(playerItem.Attack + masterItem.Attack) * masterEnchantInfo.IncreasementValue / 100);
        }
        else if (masterItem.AttributeId == 2 || masterItem.AttributeId == 3)
        {
            playerItem.Defence += (int)Math.Ceiling((double)(playerItem.Defence + masterItem.Defence) * masterEnchantInfo.IncreasementValue / 100);
        }

        return playerItem;
    }
}