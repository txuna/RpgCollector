﻿using Microsoft.AspNetCore.Mvc;
using RpgCollector.Models.AccountModel;
using RpgCollector.Models.EnchantModel;
using RpgCollector.Models.MasterModel;
using RpgCollector.Models.PlayerModel;
using RpgCollector.RequestResponseModel.PlayerItemDetailGetModel;
using RpgCollector.Services;
using ZLogger;

namespace RpgCollector.Controllers.PlayerDataController
{
    [ApiController]
    public class PlayerItemDetailGetController : Controller
    {
        ILogger<PlayerItemDetailGetController> _logger;
        IAccountMemoryDB _accountMemoryDB;
        IPlayerAccessDB _playerAccessDB;
        IMasterDataDB _masterDataDB;
        public PlayerItemDetailGetController(IPlayerAccessDB playerAccessDB, 
                                             ILogger<PlayerItemDetailGetController> logger, 
                                             IAccountMemoryDB accountMemoryDB,
                                             IMasterDataDB masterDataDB)
        {
            _playerAccessDB = playerAccessDB;
            _accountMemoryDB = accountMemoryDB;
            _logger = logger;
            _masterDataDB = masterDataDB;
        }

        [Route("/Inventory/Item")]
        [HttpPost]
        public async Task<PlayerItemDetailGetResponse> PlayerItemGetDetail(PlayerItemDetailGetRequest playerItemDetailGetRequest)
        {
            string userName = HttpContext.Request.Headers["User-Name"];
            int userId = await _accountMemoryDB.GetUserId(userName);

            _logger.ZLogInformation($"[{userId}] Request /Inventory/Item");

            if (userId == -1)
            {
                return new PlayerItemDetailGetResponse
                {
                    Error = RequestResponseModel.ErrorState.NoneExistName
                };
            }

            /* 권한 검사 */
            if(!await _playerAccessDB.IsItemOwner(playerItemDetailGetRequest.PlayerItemId, userId))
            {
                return new PlayerItemDetailGetResponse
                {
                    Error = RequestResponseModel.ErrorState.IsNotOwnerThisItem
                };
            }

            /* playerItemId 기반으로 itemId를 가지고 옴 */
            PlayerItem? playerItem = await _playerAccessDB.GetPlayerItem(playerItemDetailGetRequest.PlayerItemId);

            if(playerItem == null)
            {
                return new PlayerItemDetailGetResponse
                {
                    Error = RequestResponseModel.ErrorState.NoneExistItem
                };
            }

            /* 아이템의 프로토타입을 가지고 옴 */
            MasterItem? masterItem = _masterDataDB.GetMasterItem(playerItem.ItemId);

            if(masterItem == null)
            {
                return new PlayerItemDetailGetResponse
                {
                    Error = RequestResponseModel.ErrorState.NoneExistItem
                };
            }

            AdditionalState additionalState = new AdditionalState
            {
                Attack = 0,
                Magic = 0,
                Defence = 0,
            };

            /* 장비아이템이 아닐경우 */
            MasterItemAttribute masterItemAttribute = _masterDataDB.GetMasterItemAttribute(masterItem.AttributeId);
            MasterItemType? masterItemType = _masterDataDB.GetMasterItemType(masterItemAttribute.TypeId);

            if ((TypeDefinition)masterItemAttribute.TypeId != TypeDefinition.EQUIPMENT)
            {
                return new PlayerItemDetailGetResponse
                {
                    Error = RequestResponseModel.ErrorState.None,
                    ItemPrototype = masterItem,
                    PlusState = additionalState,
                    EnchantCount = 0,
                    AttributeName = masterItemAttribute.AttributeName,
                    TypeName = masterItemType.TypeName
                };
            }

            return new PlayerItemDetailGetResponse
            {
                Error = RequestResponseModel.ErrorState.None,
                ItemPrototype = masterItem, 
                PlusState = CalculateEnchantState(playerItem, masterItem), 
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
                // 공격력
                if (masterItem.AttributeId == 1)
                {
                    additionalState.Attack += (int)Math.Ceiling((double)(additionalState.Attack + masterItem.Attack) * masterEnchantInfo.IncreasementValue / 100);
                }
                // 방어력 
                else if (masterItem.AttributeId == 2 || masterItem.AttributeId == 3)
                {
                    additionalState.Defence += (int)Math.Ceiling((double)(additionalState.Defence + masterItem.Defence) * masterEnchantInfo.IncreasementValue / 100);
                }
            }
            
            return additionalState;
        }
    }
}
