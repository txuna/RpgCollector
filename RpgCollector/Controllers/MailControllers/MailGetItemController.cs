using Microsoft.AspNetCore.Mvc;
using RpgCollector.Models;
using RpgCollector.Models.MailModel;
using RpgCollector.RequestResponseModel;
using RpgCollector.RequestResponseModel.MailGetItemModel;
using RpgCollector.RequestResponseModel.MailReadModel;
using RpgCollector.Services;
using SqlKata;
using ZLogger;

namespace RpgCollector.Controllers.MailControllers;


[ApiController]
public class MailGetItemController : Controller
{
    IMailboxAccessDB _mailboxAccessDB;
    IPlayerAccessDB _playerAccessDB;
    ILogger<MailGetItemController> _logger;

    public MailGetItemController(IMailboxAccessDB mailboxAccessDB, 
                                 IPlayerAccessDB playerAccessDB, 
                                 ILogger<MailGetItemController> logger) 
    {
        _mailboxAccessDB = mailboxAccessDB; 
        _playerAccessDB = playerAccessDB;
        _logger = logger;
    }
/*
* 사용자가 전송한 mailId를 기반으로 아이템을 동봉하고 있는지 확인 + 이미 수령했는지 확인 및 아이템 제공
* 또한 해당 메일의 소유권자인지 확인
* 사용자의 인벤토리 업데이트
*/
    [Route("/Mail/Item")]
    [HttpPost]
    public async Task<MailGetItemResponse> GetItem(MailGetItemRequest mailGetItemRequest)
    {
        int userId = Convert.ToInt32(HttpContext.Items["User-Id"]);
        ErrorState Error;

        Mailbox? mailbox = await _mailboxAccessDB.GetMailFromUserId(mailGetItemRequest.MailId, userId);

        if(mailbox == null)
        {
            return new MailGetItemResponse
            {
                Error = ErrorState.FailedFetchMail
            };
        }

        if(mailbox.ItemId == 0 || mailbox.HasReceived == 1)
        {
            return new MailGetItemResponse
            {
                Error = ErrorState.FailedFetchMailItem
            };
        }

        Error = await AddItemToPlayer(userId, mailbox.ItemId, mailbox.Quantity, mailbox.MailId);

        if(Error != ErrorState.None)
        {
            _logger.ZLogInformation($"[{userId}] Failed Received Mail Item");
        }
        else
        {
            _logger.ZLogInformation($"[{userId}] Success Received Mail Item and Add Item to Player");
        }

        return new MailGetItemResponse
        {
            Error = Error
        };
    }

    async Task<ErrorState> AddItemToPlayer(int userId, int itemId, int quantity, int mailId)
    {
        if(!await _mailboxAccessDB.setReceiveFlagInMailItem(mailId))
        {
            Console.WriteLine("a");
            return ErrorState.CannotSetReceivedFlagInMail;
        }

        if (!await _playerAccessDB.AddItemToPlayer(userId, itemId, quantity))
        {
            Console.WriteLine("b");
            if (!await _mailboxAccessDB.UndoMailItem(mailId))
            {
                Console.WriteLine("c");
                return ErrorState.FailedUndoMailItem;
            }

            return ErrorState.FailedAddItemToPlayer;
        }

        return ErrorState.None;
    }
}
