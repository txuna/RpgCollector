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

        PlayerItem? playerItem = await _playerAccessDB.GetPlayerItem(playerItemId, userId);

        if (playerItem == null)
        {
            return new EnchantExecuteResponse
            {
                Error = ErrorState.FailedFetchPlayerItem
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

        MasterEnchantInfo masterEnchantInfo = _masterDataDB.GetMasterEnchantInfo(playerItem.EnchantCount + 1);

        Error = await Verify(playerItem, masterItem, userId, masterEnchantInfo); 

        if(Error != ErrorState.None)
        {
            return new EnchantExecuteResponse
            {
                Error = Error
            };
        }

        (Error, result) = await ExecuteEnchant(playerItem, masterItem, masterEnchantInfo);

        if(Error != ErrorState.None)
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
                Error = ErrorState.FailedFetchMoney
            };
        }
        
        return new EnchantExecuteResponse
        {
            Error = Error,
            Result = result
        };
    }

    async Task<ErrorState> Verify(PlayerItem playerItem, MasterItem masterItem, int userId, MasterEnchantInfo masterEnchantInfo)
    {
        ErrorState Error;

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

        Error = await VerifyMoney(userId, masterEnchantInfo);

        if (Error != ErrorState.None)
        {
            return Error;
        }

        return ErrorState.None;
    }

    async Task<ErrorState> VerifyMoney(int userId, MasterEnchantInfo masterEnchantInfo)
    {
        int playerMoney = await _playerAccessDB.GetPlayerMoney(userId);

        if (playerMoney < masterEnchantInfo.Price)
        {
            return ErrorState.NotEnoughMoney;
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

    (ErrorState, int) CalculateCanEnchant(MasterEnchantInfo masterEnchantInfo)
    {
        Random random = new Random();
        int randomValue = random.Next(101);
        int result = randomValue < masterEnchantInfo.Percent ? 1 : 0;

        return (ErrorState.None, result);
    }

    async Task<(ErrorState, int)> ExecuteEnchant(PlayerItem playerItem, MasterItem masterItem, MasterEnchantInfo masterEnchantInfo)
    {
        var (Error, result) = CalculateCanEnchant(masterEnchantInfo);

        if (Error != ErrorState.None)
        {
            return (Error, -1);
        }

        if (result == 1)
        {
            playerItem = CalculateIncreasementStatsValue(playerItem, masterItem, masterEnchantInfo);

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