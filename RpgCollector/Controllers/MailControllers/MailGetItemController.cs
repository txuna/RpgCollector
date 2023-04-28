using Microsoft.AspNetCore.Mvc;
using RpgCollector.Models;
using RpgCollector.Models.MailData;
using RpgCollector.RequestResponseModel;
using RpgCollector.RequestResponseModel.MailGetItemModel;
using RpgCollector.RequestResponseModel.MailReadModel;
using RpgCollector.Services;
using SqlKata;

namespace RpgCollector.Controllers.MailControllers;

public class MailGetItemController : Controller
{
    IMailboxAccessDB _mailboxAccessDB;
    IPlayerAccessDB _playerAccessDB;
    IAccountDB _accountDB;

    public MailGetItemController(IMailboxAccessDB mailboxAccessDB, IPlayerAccessDB playerAccessDB, IAccountDB accountDB) 
    {
        _mailboxAccessDB = mailboxAccessDB; 
        _playerAccessDB = playerAccessDB;
        _accountDB = accountDB;
    }
/*
* 사용자가 전송한 mailId를 기반으로 아이템을 동봉하고 있는지 확인 + 이미 수령했는지 확인 및 아이템 제공
* 또한 해당 메일의 소유권자인지 확인
* 사용자의 인벤토리 업데이트
*/
    [Route("/Mail/Item")]
    [HttpPost]
    public async Task<MailGetItemResponse> GetItem([FromBody] MailGetItemRequest readMailRequest)
    {
        if(!ModelState.IsValid)
        {
            return new MailGetItemResponse
            {
                Error = ErrorState.InvalidModel
            };
        }

        var userName = HttpContext.Request.Headers["User-Name"];
        int userId = await _accountDB.GetUserId(userName);

        if (userId == -1)
        {
            return new MailGetItemResponse
            {
                Error = ErrorState.FailedConnectRedis
            };
        }

        if (!await _mailboxAccessDB.HasMailItem(readMailRequest.MailId))
        {
            return new MailGetItemResponse
            {
                Error = ErrorState.NoneHaveItemInMail
            };
        }

        if(!await _mailboxAccessDB.IsMailOwner(readMailRequest.MailId, userId))
        {
            return new MailGetItemResponse
            {
                Error = ErrorState.NoneOwnerThisMail
            };
        }

        MailItem? mailItem = await _mailboxAccessDB.ReceiveMailItem(readMailRequest.MailId);

        if(mailItem == null)
        {
            return new MailGetItemResponse
            {
                Error = ErrorState.AlreadyReceivedItemFromMail
            };
        }

        if(!await AddItemToPlayer(userId, mailItem))
        {
            return new MailGetItemResponse
            {
                Error = ErrorState.FailedAddMailItemToPlayer
            };
        }

        return new MailGetItemResponse
        {
            Error = ErrorState.None
        };
    }

    async Task<bool> AddItemToPlayer(int userId, MailItem mailItem)
    {
        if (!await _playerAccessDB.AddItemToPlayer(userId, mailItem.ItemId, mailItem.Quantity))
        {
            if (!await _mailboxAccessDB.UndoMailItem(mailItem.MailId))
            {
                return false;
            }

            return false;
        }
        return true;
    }
}
