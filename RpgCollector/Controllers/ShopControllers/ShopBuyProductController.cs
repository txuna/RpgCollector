using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RpgCollector.Models.AccountModel;
using RpgCollector.Models.MasterModel;
using RpgCollector.RequestResponseModel.ShopReqRes;
using RpgCollector.Services;

namespace RpgCollector.Controllers.ShopControllers;

[ApiController]
public class ShopBuyProductController : Controller
{
    IMasterDataDB _masterDataDB;
    ILogger<ShopBuyProductController> _logger;
    IPlayerAccessDB _playerAccessDB;
    IMailboxAccessDB _mailboxAccessDB;
    public ShopBuyProductController(IMasterDataDB masterDataDB, 
                                    ILogger<ShopBuyProductController> logger, 
                                    IPlayerAccessDB playerAccessDB, 
                                    IMailboxAccessDB mailboxAccessDB)
    {
        _logger = logger;
        _masterDataDB = masterDataDB;
        _playerAccessDB = playerAccessDB;
        _mailboxAccessDB = mailboxAccessDB;
    }

    [Route("/Shop/Buy")]
    [HttpPost]
    public async Task<ShopBuyProductResponse> Post(ShopBuyProductRequest shopBuyProductRequest)
    {
        RedisUser redisUser = (RedisUser)HttpContext.Items["Redis-User"];

        MasterItem? masterItem = LoadMasterItem(shopBuyProductRequest.ItemId);
        if(masterItem == null)
        {
            return new ShopBuyProductResponse
            {
                Error = RequestResponseModel.ErrorCode.NoneExistItem
            };
        }

        if(await IsEnoughPlayerMoney(redisUser, masterItem.BuyPrice) == false)
        {
            return new ShopBuyProductResponse
            {
                Error = RequestResponseModel.ErrorCode.NotEnoughMoney
            };
        }

        if(await BuyItem(redisUser, masterItem) == false)
        {
            return new ShopBuyProductResponse
            {
                Error = RequestResponseModel.ErrorCode.FailedBuyItem
            };
        }

        return new ShopBuyProductResponse
        {
            Error = RequestResponseModel.ErrorCode.None
        };
    }

    MasterItem? LoadMasterItem(int itemId)
    {
        MasterItem? masterItem = _masterDataDB.GetMasterItem(itemId);
        return masterItem;
    }

    async Task<bool> IsEnoughPlayerMoney(RedisUser redisUser, int itemPrice)
    {
        int playerMoney = await _playerAccessDB.GetPlayerMoney(redisUser.UserId);
        if(playerMoney < itemPrice)
        {
            return false;
        }

        return true;
    }

    async Task<bool> BuyItem(RedisUser redisUser, MasterItem masterItem)
    {
        if(await _playerAccessDB.SubtractionMoneyToPlayer(redisUser.UserId, masterItem.BuyPrice) == false)
        {
            return false;
        }

        if (await _mailboxAccessDB.SendMail(1,  redisUser.UserId, "Completed Shipping Item", "Thank you for purchase", masterItem.ItemId, 1) == false)
        {
            if (await _playerAccessDB.AddMoneyToPlayer(redisUser.UserId, masterItem.BuyPrice) == false)
            {
                return false;
            }

            return false;
        }

        return true;
    }
}
