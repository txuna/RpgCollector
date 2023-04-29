using Microsoft.AspNetCore.Mvc;
using RpgCollector.RequestResponseModel.EnchantModel;
using RpgCollector.RequestResponseModel;
using RpgCollector.Services;
using RpgCollector.Models.MasterData;
using RpgCollector.Models;

namespace RpgCollector.Controllers.EnchantControllers;

[ApiController]
public class EnchantExecuteController : Controller
{
    IEnchantDB _enchantDB; 
    IAccountDB _accountDB;

    public EnchantExecuteController(IEnchantDB enchantDB, IAccountDB accountDB)
    {
        _enchantDB = enchantDB;
        _accountDB = accountDB;
    }

    /*
    1. TypeDefinition 확인 (장비아이템만) 
    2. EchantCount와 MaxEnchatCount 비교 
    3.다음번에 갈 강화테이블 참조 
    3.1 강화 참조 테이블 
    EnchantCountId - Percent - 
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

        Error = await VerifyItemPermission(playerItemId); 
        if(Error != ErrorState.None)
        {
            return new EnchantExecuteResponse
            {
                Error = Error
            };
        }

        PlayerItem? playerItem = await _enchantDB.GetPlayerItem(playerItemId);

        if(playerItem == null)
        {
            return new EnchantExecuteResponse
            {
                Error = ErrorState.NoneExistItem
            };
        }

        MasterItem? masterItem = await _enchantDB.GetMasterItem(playerItem.ItemId);

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

        Error = await VerifyEnchatMaxCount(playerItem, masterItem);

        if(Error != ErrorState.None)
        {
            return new EnchantExecuteResponse
            {
                Error = Error
            };
        }

        return new EnchantExecuteResponse
        {
            Error = ErrorState.None
        };
    }

    async Task<ErrorState> VerifyItemPermission(int playerItemId)
    {
        string userName = HttpContext.Request.Headers["User-Name"];
        int userId = await _accountDB.GetUserId(userName);

        if(userId == -1)
        {
            return ErrorState.NoneExistName;
        }

        if(!await _enchantDB.IsUserHasItem(playerItemId, userId))
        {
            return ErrorState.IsNotOwnerThisItem;
        }

        return ErrorState.None;
    }

    async Task<ErrorState> VerifyItemType(int attributeId)
    {
        TypeDefinition itemType = await _enchantDB.GetItemType(attributeId);

        if(itemType != TypeDefinition.EQUIPMENT)
        {
            return ErrorState.CantNotEnchantThisType;
        }

        return ErrorState.None;
    }

    async Task<ErrorState> VerifyEnchatMaxCount(PlayerItem playerItem, MasterItem masterItem)
    {
        if(playerItem.EnchantCount >= masterItem.MaxEnchantCount)
        {
            return ErrorState.AlreadyMaxiumEnchantCount;
        }
        return ErrorState.None;
    }

    async Task<ErrorState> ExecuteEnchant()
    {

    }
}
