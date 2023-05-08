using Microsoft.AspNetCore.Mvc;
using RpgCollector.Models.EnchantModel;
using RpgCollector.Models.MasterModel;
using RpgCollector.Models.PlayerModel;
using RpgCollector.RequestResponseModel;
using RpgCollector.RequestResponseModel.EnchantInfoGet;
using RpgCollector.RequestResponseModel.EnchantModel;
using RpgCollector.Services;
using ZLogger;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace RpgCollector.Controllers.EnchantControllers
{
    [ApiController]
    public class EnchantInfoGetController : Controller
    {
        ILogger<EnchantInfoGetController> _logger;
        IAccountMemoryDB _accountMemoryDB;
        IMasterDataDB _masterDataDB;
        IPlayerAccessDB _playerAccessDB;
        public EnchantInfoGetController(IMasterDataDB masterDataDB, ILogger<EnchantInfoGetController> logger, IAccountMemoryDB accountMemoryDB, IPlayerAccessDB playerAccessDB)
        {
            _masterDataDB = masterDataDB;
            _accountMemoryDB = accountMemoryDB;
            _logger = logger;
            _playerAccessDB = playerAccessDB;
        }

        [Route("/Enchant/Info")]
        [HttpPost]
        public async Task<EnchantInfoGetResponse> EnchantInfoGet(EnchantInfoGetRequest enchantInfoGetRequest)
        {
            string userName = HttpContext.Request.Headers["User-Name"];
            int userId = await _accountMemoryDB.GetUserId(userName);
            ErrorState Error;

            _logger.ZLogInformation($"[{userId}] Request /Enchant/Info ");

            if(userId == -1)
            {
                return new EnchantInfoGetResponse
                {
                    Error = ErrorState.NoneExistName
                };
            }

            /* 플레이어의 아이템 로드 */
            PlayerItem? playerItem = await _playerAccessDB.GetPlayerItem(enchantInfoGetRequest.PlayerItemId);

            if (playerItem == null)
            {
                return new EnchantInfoGetResponse
                {
                    Error = ErrorState.NoneExistItem
                };
            }
            /* ItemId를 기반으로 마스터 아이템 로드 */
            MasterItem? masterItem = _masterDataDB.GetMasterItem(playerItem.ItemId);

            if (masterItem == null)
            {
                return new EnchantInfoGetResponse
                {
                    Error = ErrorState.NoneExistItem
                };
            }

            Error = await Verify(playerItem, masterItem, userId);
            if (Error != ErrorState.None)
            {
                return new EnchantInfoGetResponse
                {
                    Error = Error
                };
            }

            /* player의 enchant count와 next enchant count 체킹 */
            MasterEnchantInfo masterEnchantInfo = _masterDataDB.GetMasterEnchantInfo(playerItem.EnchantCount+1);

            return new EnchantInfoGetResponse
            {
                Error = ErrorState.None,
                CurrentEnchantCount = playerItem.EnchantCount,
                NextEnchantCount = playerItem.EnchantCount + 1, 
                Percent = masterEnchantInfo.Percent, 
                IncreasementValue = masterEnchantInfo.IncreasementValue,
                ItemId = playerItem.ItemId,
                PlayerItemId = playerItem.PlayerItemId,
            };
        }

        async Task<ErrorState> Verify(PlayerItem playerItem, MasterItem masterItem, int userId)
        {
            ErrorState Error;
            Error = await VerifyItemPermission(playerItem.PlayerItemId, userId);
            if (Error != ErrorState.None)
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
    }
}
