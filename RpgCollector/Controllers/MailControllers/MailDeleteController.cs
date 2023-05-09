using Microsoft.AspNetCore.Mvc;
using RpgCollector.RequestResponseModel;
using RpgCollector.RequestResponseModel.MailDeleteModel;
using RpgCollector.Services;

namespace RpgCollector.Controllers.MailControllers;

[ApiController]
public class MailDeleteController : Controller
{
    IAccountMemoryDB _accountMemoryDB;
    ILogger<MailDeleteController> _logger;
    IMailboxAccessDB _mailboxAccessDB;
    public MailDeleteController(IMailboxAccessDB mailboxAccessDB, 
                                ILogger<MailDeleteController> logger, 
                                IAccountMemoryDB accountMemoryDB)
    {
        _mailboxAccessDB = mailboxAccessDB;
        _logger = logger;
        _accountMemoryDB = accountMemoryDB;
    }

    [Route("/Mail/Delete")]
    [HttpPost]
    public async Task<MailDeleteResponse> DeleteMail(MailDeleteRequest mailDeleteRequest)
    {
        string userName = HttpContext.Request.Headers["User-Name"];
        int userId = await _accountMemoryDB.GetUserId(userName);
        ErrorState Error;

        Error = await Verify(mailDeleteRequest.MailId, userId); 

        if(Error != ErrorState.None)
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
