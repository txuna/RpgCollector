using Microsoft.AspNetCore.Mvc;
using RpgCollector.Models.MailModel;
using RpgCollector.RequestResponseModel;
using RpgCollector.RequestResponseModel.MailReqRes;
using RpgCollector.Services;
using ZLogger;

namespace RpgCollector.Controllers.MailControllers;

[ApiController]
public class MailDeleteController : Controller
{
    ILogger<MailDeleteController> _logger;
    IMailboxAccessDB _mailboxAccessDB;
    public MailDeleteController(IMailboxAccessDB mailboxAccessDB, 
                                ILogger<MailDeleteController> logger)
    {
        _mailboxAccessDB = mailboxAccessDB;
        _logger = logger;
    }

    [Route("/Mail/Delete")]
    [HttpPost]
    public async Task<MailDeleteResponse> DeleteMail(MailDeleteRequest mailDeleteRequest)
    {
        int userId = Convert.ToInt32(HttpContext.Items["User-Id"]);
        ErrorCode Error;

        _logger.ZLogDebug($"[{userId}] Request /Mail/Delete");

        Error = await Verify(mailDeleteRequest.MailId, userId);

        if(Error != ErrorCode.None)
        {
            return new MailDeleteResponse
            {
                Error = Error
            };
        }

        Error = await ExecuteDelete(mailDeleteRequest.MailId);

        return new MailDeleteResponse
        {
            Error = Error
        };
    }

    async Task<ErrorCode> ExecuteDelete(int mailId)
    {
        if(await _mailboxAccessDB.DeleteMail(mailId) == false)
        {
            return ErrorCode.FailedDeleteMail;
        }

        return ErrorCode.None;
    }

    async Task<ErrorCode> Verify(int mailId, int userId)
    {
        Mailbox? mailbox = await _mailboxAccessDB.GetMailFromUserId(mailId, userId);

        if(mailbox == null)
        {
            return ErrorCode.FailedFetchMail;
        }

        if(mailbox.IsDeleted == 1)
        {
            return ErrorCode.DeletedMail;
        }

        return ErrorCode.None;
    }
}
