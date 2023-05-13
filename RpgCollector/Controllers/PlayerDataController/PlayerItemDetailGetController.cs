using Microsoft.AspNetCore.Mvc;
using RpgCollector.Models.AccountModel;
using RpgCollector.Models.EnchantModel;
using RpgCollector.Models.MasterModel;
using RpgCollector.Models.PlayerModel;
using RpgCollector.RequestResponseModel.PlayerReqRes;
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

        PlayerItem? playerItem = await _playerAccessDB.GetPlayerItem(playerItemDetailGetRequest.PlayerItemId, userId);

        if(playerItem == null)
        {
            return new PlayerItemDetailGetResponse
            {
                Error = RequestResponseModel.ErrorCode.NoneExistItem
            };
        }

        MasterItem? masterItem = _masterDataDB.GetMasterItem(playerItem.ItemId);

        if(masterItem == null)
        {
            return new PlayerItemDetailGetResponse
            {
                Error = RequestResponseModel.ErrorCode.NoneExistItem
            };
        }

        MasterItemAttribute? masterItemAttribute = _masterDataDB.GetMasterItemAttribute(masterItem.AttributeId);
        MasterItemType? masterItemType = _masterDataDB.GetMasterItemType(masterItemAttribute.TypeId);

        return new PlayerItemDetailGetResponse
        {
            Error = RequestResponseModel.ErrorCode.None,
            ItemId = playerItem.ItemId,
            ItemName = masterItem.ItemName,
            BaseAttack = masterItem.Attack,
            BaseDefence = masterItem.Defence,
            BaseMagic = masterItem.Magic,
            PlusAttack = playerItem.Attack, 
            PlusDefence = playerItem.Defence,
            PlusMagic = playerItem.Magic,
            EnchantCount = playerItem.EnchantCount,
            AttributeName = masterItemAttribute.AttributeName,
            TypeName = masterItemType.TypeName
        };
    }
}
