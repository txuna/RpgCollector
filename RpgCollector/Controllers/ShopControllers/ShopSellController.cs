using Microsoft.AspNetCore.Mvc;
using RpgCollector.Models.AccountModel;
using RpgCollector.Models.MasterModel;
using RpgCollector.Models.PlayerModel;
using RpgCollector.RequestResponseModel.ShopReqRes;
using RpgCollector.Services;

namespace RpgCollector.Controllers.ShopControllers
{
    [ApiController]
    public class ShopSellController : Controller
    {
        ILogger<ShopSellController> _logger;
        IPlayerAccessDB _playerAccessDB;
        IMasterDataDB _masterDataDB;
        public ShopSellController(ILogger<ShopSellController> logger, IPlayerAccessDB playerAccessDB, IMasterDataDB masterDataDB)
        {
            _logger = logger;
            _playerAccessDB = playerAccessDB;
            _masterDataDB = masterDataDB;
        }


        [Route("/Shop/Sell")]
        [HttpPost]
        public async Task<ShopSellProductResponse> Post(ShopSellProductRequest shopSellProductRequest)
        {
            RedisUser redisUser = (RedisUser)HttpContext.Items["Redis-User"];

            // load player item 
            PlayerItem? playerItem = await LoadPlayerItem(shopSellProductRequest.PlayerItemId, redisUser.UserId);
            if(playerItem == null)
            {
                return new ShopSellProductResponse
                {
                    Error = RequestResponseModel.ErrorCode.NoneExistItem
                };
            }

            // load master item 
            MasterItem? masterItem = LoadMasterItem(playerItem.ItemId);
            if(masterItem == null)
            {
                return new ShopSellProductResponse
                {
                    Error = RequestResponseModel.ErrorCode.NoneExistItem
                };
            }

            // sell item and sub money
            if(await SellProduct(redisUser.UserId, playerItem.PlayerItemId, masterItem.SellPrice) == false)
            {
                return new ShopSellProductResponse
                {
                    Error = RequestResponseModel.ErrorCode.FailedSellItem
                };
            }

            return new ShopSellProductResponse
            {
                Error = RequestResponseModel.ErrorCode.None
            };
        }

        async Task<PlayerItem?> LoadPlayerItem(int playerItemId, int userId)
        {
            PlayerItem? playerItem = await _playerAccessDB.GetPlayerItem(playerItemId, userId);
            return playerItem; 
        }

        MasterItem? LoadMasterItem(int itemId)
        {
            MasterItem? masterItem = _masterDataDB.GetMasterItem(itemId);
            return masterItem;
        }

        async Task<bool> SellProduct(int userId, int playerItemId, int price)
        {
            if(await _playerAccessDB.AddMoneyToPlayer(userId, price) == false)
            {
                return false;
            }

            if(await _playerAccessDB.RemovePlayerItem(playerItemId) == false)
            {
                if(await _playerAccessDB.SubtractionMoneyToPlayer(userId, price) == false)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
