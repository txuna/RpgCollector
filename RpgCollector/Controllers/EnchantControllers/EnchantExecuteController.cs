using Microsoft.AspNetCore.Mvc;
using RpgCollector.RequestResponseModel.EnchantModel;
using RpgCollector.RequestResponseModel;
using RpgCollector.Services;
using RpgCollector.Models.MasterData;
using RpgCollector.Models;
using RpgCollector.Models.EnchantModel;

namespace RpgCollector.Controllers.EnchantControllers;

[ApiController]
public class EnchantExecuteController : Controller
{
    IEnchantDB _enchantDB; 
    IAccountDB _accountDB;
    IPlayerAccessDB _playerAccessDB;

    public EnchantExecuteController(IEnchantDB enchantDB, IAccountDB accountDB, IPlayerAccessDB playerAccessDB)
    {
        _enchantDB = enchantDB;
        _accountDB = accountDB;
        _playerAccessDB = playerAccessDB;
    }

    /*
    1. TypeDefinition 확인 (장비아이템만) 
    2. EchantCount와 MaxEnchatCount 비교 
    3.다음번에 갈 강화테이블 참조 
    3.1 강화 참조 테이블 
    EnchantCountId - Percent - increasementValue
    4. 실패시 해당 아이템 삭제 
    5. 강화 이력 테이블 갱신 
    logId - userId - playerItemid - isSuccess - date 
    실제 전투시 강화 테이블을 참조하여 데미지 계산 - 4성이면 프로토타입에서 10% 4번 곱하기
     */
    [Route("/Enchant")]
    [HttpPost]
    public async Task<EnchantExecuteResponse> Enchant(EnchantExecuteRequest enchantExecuteRequest)
    {
        int playerItemId = enchantExecuteRequest.PlayerItemId;
        ErrorState Error;
        int result;

        string userName = HttpContext.Request.Headers["User-Name"];
        int userId = await _accountDB.GetUserId(userName);

        Error = await VerifyItemPermission(playerItemId, userId); 
        if(Error != ErrorState.None)
        {
            return new EnchantExecuteResponse
            {
                Error = Error
            };
        }

        PlayerItem? playerItem = await _playerAccessDB.GetPlayerItem(playerItemId);

        if(playerItem == null)
        {
            return new EnchantExecuteResponse
            {
                Error = ErrorState.NoneExistItem
            };
        }

        MasterItem? masterItem = await _playerAccessDB.GetMasterItemFromItemId(playerItem.ItemId);

        if(masterItem == null)
        {
            return new EnchantExecuteResponse
            {
                Error = ErrorState.NoneExistItem
            };
        }

        Error = await VerifyItemType(masterItem.AttributeId);

        if(Error != ErrorState.None)
        {
            return new EnchantExecuteResponse
            {
                Error = Error
            };
        }

        Error = VerifyEnchatMaxCount(playerItem, masterItem);

        if(Error != ErrorState.None)
        {
            return new EnchantExecuteResponse
            {
                Error = Error
            };
        }

        (Error, result) = await ExecuteEnchant(playerItem, masterItem, userId);

        if(Error != ErrorState.None)
        {
            return new EnchantExecuteResponse
            {
                Error = Error
            };
        }

        return new EnchantExecuteResponse
        {
            Error = ErrorState.None,
            Result = result
        };
    }

    async Task<ErrorState> VerifyItemPermission(int playerItemId, int userId)
    {
        if(!await _enchantDB.IsUserHasItem(playerItemId, userId))
        {
            return ErrorState.IsNotOwnerThisItem;
        }

        return ErrorState.None;
    }

    async Task<ErrorState> VerifyItemType(int attributeId)
    {
        TypeDefinition itemType = await _playerAccessDB.GetItemType(attributeId);

        if(itemType != TypeDefinition.EQUIPMENT)
        {
            return ErrorState.CantNotEnchantThisType;
        }

        return ErrorState.None;
    }

    ErrorState VerifyEnchatMaxCount(PlayerItem playerItem, MasterItem masterItem)
    {
        if(playerItem.EnchantCount >= masterItem.MaxEnchantCount)
        {
            return ErrorState.AlreadyMaxiumEnchantCount;
        }
        return ErrorState.None;
    }

    /*
     확률을 참고하며 실패시 아이템 삭제
     */
    async Task<(ErrorState, int)> ExecuteEnchant(PlayerItem playerItem, MasterItem masterItem, int userId)
    {
        // 현재 강화 진행 상태에 따른 강화확률에 따라강화 진행 
        MasterEnchantInfo? masterEnchantInfo = await _enchantDB.GetEnchantInfo(playerItem.EnchantCount);
        if(masterEnchantInfo == null)
        {
            return (ErrorState.NoneExistEnchantCount, -1);
        }

        Random random = new Random();
        int randomValue = random.Next(101);
        int result = randomValue < masterEnchantInfo.Percent ? 1 : 0;
        // 강화 실패
        if (result == 0)
        {
            if(!await _playerAccessDB.RemovePlayerItem(playerItem.PlayerItemId))
            {
                return (ErrorState.NoneExistItem, -1);
            }
        }
        else
        {
            if (!await _enchantDB.DoEnchant(playerItem))
            {
                return (ErrorState.NoneExistItem, -1);
            }   
        }

        return await LogEnchant(playerItem, userId, result);
    }

    async Task<(ErrorState, int)> LogEnchant(PlayerItem playerItem, int userId, int result)
    {
        if(!await _enchantDB.EnchantLog(playerItem.PlayerItemId, userId, playerItem.EnchantCount, result))
        {
            return (ErrorState.FailedLogEnchant, -1);
        }
        return (ErrorState.None, result); 
    }
}
