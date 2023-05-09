using Microsoft.AspNetCore.Mvc;
using RpgCollector.RequestResponseModel;
using RpgCollector.RequestResponseModel.MailDeleteModel;
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
        string userName = HttpContext.Request.Headers["User-Name"];
        int userId = Convert.ToInt32(HttpContext.Items["User-Id"]);
        ErrorState Error;

        _logger.ZLogInformation($"[{userId}] Request /Mail/Delete");

        Error = await Verify(mailDeleteRequest.MailId, userId); 

        if(Error != ErrorState.None)
        {
            _logger.ZLogInformation($"[{userId}] None Have Permission Delete Mail : {mailDeleteRequest.MailId}");

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

    async Task<ErrorState> ExecuteDelete(int mailId)
    {
        if(!await _mailboxAccessDB.DeleteMail(mailId))
        {
            return ErrorState.FailedDeleteMail;
        }

        return ErrorState.None;
    }

    async Task<ErrorState> Verify(int mailId, int userId)
    {
        if (await _mailboxAccessDB.IsDeadLine(mailId))
        {
            return ErrorState.AlreadyMailDeadlineExpireDate;
        }

        if (!await _mailboxAccessDB.IsMailOwner(mailId, userId))
        {
            return ErrorState.NoneOwnerThisMail;
        }

        if(await _mailboxAccessDB.IsDeletedMail(mailId))
        {
            return ErrorState.DeletedMail;
        }

        return ErrorState.None;
    }
}
