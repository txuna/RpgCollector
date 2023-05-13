using Microsoft.AspNetCore.Mvc;
using RpgCollector.Models;
using RpgCollector.Models.MailModel;
using RpgCollector.RequestResponseModel;
using RpgCollector.RequestResponseModel.MailReqRes;
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

    [Route("/Mail/Item")]
    [HttpPost]
    public async Task<MailGetItemResponse> GetItem(MailGetItemRequest mailGetItemRequest)
    {
        int userId = Convert.ToInt32(HttpContext.Items["User-Id"]);
        ErrorCode Error;

        Mailbox? mailbox = await _mailboxAccessDB.GetMailFromUserId(mailGetItemRequest.MailId, userId);

        if(mailbox == null)
        {
            return new MailGetItemResponse
            {
                Error = ErrorCode.FailedFetchMail
            };
        }

        if(mailbox.ItemId == 0 || mailbox.HasReceived == 1)
        {
            return new MailGetItemResponse
            {
                Error = ErrorCode.FailedFetchMailItem
            };
        }

        Error = await AddItemToPlayer(userId, mailbox.ItemId, mailbox.Quantity, mailbox.MailId);

        if(Error != ErrorCode.None)
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

    async Task<ErrorCode> AddItemToPlayer(int userId, int itemId, int quantity, int mailId)
    {
        if(await _mailboxAccessDB.setReceiveFlagInMailItem(mailId) == false)
        {
            return ErrorCode.CannotSetReceivedFlagInMail;
        }

        if (await _playerAccessDB.AddItemToPlayer(userId, itemId, quantity) == false)
        {
            if (await _mailboxAccessDB.UndoMailItem(mailId) == false)
            {
                return ErrorCode.FailedUndoMailItem;
            }

            return ErrorCode.FailedAddItemToPlayer;
        }

        return ErrorCode.None;
    }
}
