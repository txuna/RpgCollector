using Microsoft.AspNetCore.Mvc;
using RpgCollector.Models.AccountModel;
using RpgCollector.Models.EnchantModel;
using RpgCollector.Models.MasterModel;
using RpgCollector.Models.PlayerModel;
using RpgCollector.RequestResponseModel.PlayerItemDetailGetModel;
using RpgCollector.Services;
using ZLogger;

namespace RpgCollector.Controllers.PlayerDataController;

[ApiController]
public class PlayerItemDetailGetController : Controller
{
    ILogger<PlayerItemDetailGetController> _logger;
    IPlayerAccessDB _playerAccessDB;
    IMasterDataDB _masterDataDB;
    public PlayerItemDetailGetController(IPlayerAccessDB playerAccessDB, 
                                         ILogger<PlayerItemDetailGetController> logger, 
                                         IMasterDataDB masterDataDB)
    {
        _playerAccessDB = playerAccessDB;
        _logger = logger;
        _masterDataDB = masterDataDB;
    }

    [Route("/Inventory/Item")]
    [HttpPost]
    public async Task<PlayerItemDetailGetResponse> PlayerItemGetDetail(PlayerItemDetailGetRequest playerItemDetailGetRequest)
    {
        int userId = Convert.ToInt32(HttpContext.Items["User-Id"]);

        _logger.ZLogInformation($"[{userId}] Request /Inventory/Item");

        if(!await _playerAccessDB.IsItemOwner(playerItemDetailGetRequest.PlayerItemId, userId))
        {
            return new PlayerItemDetailGetResponse
            {
                Error = RequestResponseModel.ErrorState.IsNotOwnerThisItem
            };
        }

        PlayerItem? playerItem = await _playerAccessDB.GetPlayerItem(playerItemDetailGetRequest.PlayerItemId);

        if(playerItem == null)
        {
            return new PlayerItemDetailGetResponse
            {
                Error = RequestResponseModel.ErrorState.NoneExistItem
            };
        }

        MasterItem? masterItem = _masterDataDB.GetMasterItem(playerItem.ItemId);

        if(masterItem == null)
        {
            return new PlayerItemDetailGetResponse
            {
                Error = RequestResponseModel.ErrorState.NoneExistItem
            };
        }

        MasterItemAttribute masterItemAttribute = _masterDataDB.GetMasterItemAttribute(masterItem.AttributeId);
        MasterItemType? masterItemType = _masterDataDB.GetMasterItemType(masterItemAttribute.TypeId);
        AdditionalState additionalState;

        if ((TypeDefinition)masterItemAttribute.TypeId != TypeDefinition.EQUIPMENT)
        {
            additionalState = new AdditionalState
            {
                Attack = 0,
                Magic = 0,
                Defence = 0
            };
        }
        else
        {
            additionalState = CalculateEnchantState(playerItem, masterItem);
        }

        return new PlayerItemDetailGetResponse
        {
            Error = RequestResponseModel.ErrorState.None,
            ItemPrototype = masterItem, 
            PlusState = additionalState, 
            EnchantCount = playerItem.EnchantCount,
            AttributeName = masterItemAttribute.AttributeName,
            TypeName = masterItemType.TypeName
        };
    }

    /* 
        * 플레이어 아이템의 강화 횟수만큼 - 능력치 뻥튀기 
        * 방어구는 방어력, 무기는 마법력과 공력력 master_enchant_info table 참고 
        * ex) 4성이라면 1성 테이블 참고, 2성 테이블 참고 ... 4성 테이블 참고 이런식으로 진행
    */
    AdditionalState CalculateEnchantState(PlayerItem playerItem, MasterItem masterItem)
    {
        AdditionalState additionalState = new AdditionalState
        {
            Attack = 0,
            Defence = 0,
            Magic = 0,
        };

        for (int i = 1; i <= playerItem.EnchantCount; i++)
        {
            MasterEnchantInfo masterEnchantInfo = _masterDataDB.GetMasterEnchantInfo(i);

            if (masterItem.AttributeId == 1)
            {
                additionalState.Attack += (int)Math.Ceiling((double)(additionalState.Attack + masterItem.Attack) * masterEnchantInfo.IncreasementValue / 100);
            }
            else if (masterItem.AttributeId == 2 || masterItem.AttributeId == 3)
            {
                additionalState.Defence += (int)Math.Ceiling((double)(additionalState.Defence + masterItem.Defence) * masterEnchantInfo.IncreasementValue / 100);
            }
        }
        
        return additionalState;
    }
}
