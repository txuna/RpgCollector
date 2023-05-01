using Microsoft.AspNetCore.Mvc;
using RpgCollector.Models;
using RpgCollector.Models.MailData;
using RpgCollector.RequestResponseModel;
using RpgCollector.RequestResponseModel.MailGetItemModel;
using RpgCollector.RequestResponseModel.MailReadModel;
using RpgCollector.Services;
using SqlKata;
using ZLogger;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace RpgCollector.Controllers.MailControllers;


[ApiController]
public class MailGetItemController : Controller
{
    IMailboxAccessDB _mailboxAccessDB;
    IPlayerAccessDB _playerAccessDB;
    IAccountDB _accountDB;
    ILogger<MailGetItemController> _logger;

    public MailGetItemController(IMailboxAccessDB mailboxAccessDB, IPlayerAccessDB playerAccessDB, IAccountDB accountDB, ILogger<MailGetItemController> logger) 
    {
        _mailboxAccessDB = mailboxAccessDB; 
        _playerAccessDB = playerAccessDB;
        _accountDB = accountDB;
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
        var userName = HttpContext.Request.Headers["User-Name"];
        ErrorState Error; 

        int userId = await _accountDB.GetUserId(userName);

        _logger.ZLogInformation($"[{userId} {userName}] Request 'Get Mail Item'");

        if (userId == -1)
        {
            _logger.ZLogInformation($"[{userName}] None Exist");

            return new MailGetItemResponse
            {
                Error = ErrorState.FailedConnectRedis
            };
        }

        Error = await VerifyMail(mailGetItemRequest, userId);

        if(Error != ErrorState.None)
        {
            _logger.ZLogInformation($"[{userId} {userName}] None Have Permission This Mail {mailGetItemRequest.MailId}");

            return new MailGetItemResponse
            {
                Error = Error
            };
        }

        Error = await AddItemToPlayer(userId, mailGetItemRequest.MailId);

        if(Error != ErrorState.None)
        {
            _logger.ZLogInformation($"[{userId} {userName}] Failed Received Mail Item");
        }
        else
        {
            _logger.ZLogInformation($"[{userId} {userName}] Success Received Mail Item and Add Item to Player");
        }

        return new MailGetItemResponse
        {
            Error = Error
        };
    }

    async Task<ErrorState> VerifyMail(MailGetItemRequest mailGetItemRequest, int userId)
    {
        if (!await _mailboxAccessDB.HasMailItem(mailGetItemRequest.MailId))
        {
            return ErrorState.NoneHaveItemInMail;
        }

        if (!await _mailboxAccessDB.IsMailOwner(mailGetItemRequest.MailId, userId))
        {
            return ErrorState.NoneOwnerThisMail;
        }

        return ErrorState.None;
    }

    async Task<ErrorState> AddItemToPlayer(int userId, int mailId)
    {
        MailItem? mailItem = await _mailboxAccessDB.ReceiveMailItem(mailId);

        if (mailItem == null)
        {
            return ErrorState.NoneExistMail;
        }

        if (!await _playerAccessDB.AddItemToPlayer(userId, mailItem.ItemId, mailItem.Quantity))
        {
            if (!await _mailboxAccessDB.UndoMailItem(mailItem.MailId))
            {
                return ErrorState.FailedUndoMailItem;
            }

            return ErrorState.FailedAddItemToPlayer;
        }

        return ErrorState.None;
    }
}
