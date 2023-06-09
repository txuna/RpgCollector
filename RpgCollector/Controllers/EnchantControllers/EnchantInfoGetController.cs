﻿using Microsoft.AspNetCore.Mvc;
using RpgCollector.Models.MasterModel;
using RpgCollector.Models.PlayerModel;
using RpgCollector.RequestResponseModel;
using RpgCollector.RequestResponseModel.EnchantReqRes;
using RpgCollector.Services;
using ZLogger;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace RpgCollector.Controllers.EnchantControllers;

[ApiController]
public class EnchantInfoGetController : Controller
{
    ILogger<EnchantInfoGetController> _logger;
    IMasterDataDB _masterDataDB;
    IPlayerAccessDB _playerAccessDB;
    public EnchantInfoGetController(IMasterDataDB masterDataDB, ILogger<EnchantInfoGetController> logger, IPlayerAccessDB playerAccessDB)
    {
        _masterDataDB = masterDataDB;
        _logger = logger;
        _playerAccessDB = playerAccessDB;
    }

    [Route("/Enchant/Info")]
    [HttpPost]
    public async Task<EnchantInfoGetResponse> EnchantInfoGet(EnchantInfoGetRequest enchantInfoGetRequest)
    {
        int userId = Convert.ToInt32(HttpContext.Items["User-Id"]);
        ErrorCode Error;

        _logger.ZLogDebug($"[{userId}] Request /Enchant/Info");

        PlayerItem? playerItem = await _playerAccessDB.GetPlayerItem(enchantInfoGetRequest.PlayerItemId, userId);

        if (playerItem == null)
        {
            return new EnchantInfoGetResponse
            {
                Error = ErrorCode.NoneExistItem
            };
        }

        MasterItem? masterItem = _masterDataDB.GetMasterItem(playerItem.ItemId);

        if (masterItem == null)
        {
            return new EnchantInfoGetResponse
            {
                Error = ErrorCode.NoneExistItem
            };
        }

        Error = Verify(playerItem, masterItem);

        if (Error != ErrorCode.None)
        {
            return new EnchantInfoGetResponse
            {
                Error = Error
            };
        }

        MasterEnchantInfo masterEnchantInfo = _masterDataDB.GetMasterEnchantInfo(playerItem.EnchantCount+1);

        return new EnchantInfoGetResponse
        {
            Error = ErrorCode.None,
            CurrentEnchantCount = playerItem.EnchantCount,
            NextEnchantCount = playerItem.EnchantCount + 1, 
            Percent = masterEnchantInfo.Percent, 
            IncreasementValue = masterEnchantInfo.IncreasementValue,
            ItemId = playerItem.ItemId,
            PlayerItemId = playerItem.PlayerItemId,
            Price = masterEnchantInfo.Price,
        };
    }

    ErrorCode Verify(PlayerItem playerItem, MasterItem masterItem)
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
}
