using Microsoft.AspNetCore.Mvc;
using RpgCollector.Models.MailModel;
using RpgCollector.RequestResponseModel.MailReadModel;
using RpgCollector.RequestResponseModel;
using RpgCollector.Services;
using ZLogger;

namespace RpgCollector.Controllers.MailControllers;

[ApiController]
public class MailReadController : Controller
{
    IMailboxAccessDB _mailboxAccessDB;
    IAccountMemoryDB _accountMemoryDB;
    ILogger<MailReadController> _logger;

    public MailReadController(IMailboxAccessDB mailboxAccessDB, 
                              IAccountMemoryDB accountMemoryDB,
                              ILogger<MailReadController> logger)
    {
        _mailboxAccessDB = mailboxAccessDB;
        _logger = logger;
        _accountMemoryDB = accountMemoryDB;
    }

    /*
     * MailId를 기반으로 해당 메일 전송 Title과 Content
     * 읽음 처리 진행 
     */
    [Route("/Mail/Read")]
    [HttpPost]
    public async Task<MailReadResponse> ReadMail(MailReadRequest readMailRequest)
    {
        string userName = HttpContext.Request.Headers["User-Name"];
        int userId = await _accountMemoryDB.GetUserId(userName);
        ErrorState Error;

        _logger.ZLogInformation($"[{userId} {userName}] Request 'Read Mail'");

        if (userId == -1)
        {
            return new MailReadResponse
            {
                Error = ErrorState.NoneExistName
            };
        }

        /* 메일 유효성 검사 */
        Error = await Verify(readMailRequest.MailId, userId);

        if(Error != ErrorState.None)
        {
            return new MailReadResponse
            {
                Error = Error
            };
        }

        /* 해당 mailId를 기반으로 메일을 가지고옴 */
        Mailbox? mail = await _mailboxAccessDB.GetMailFromUserId(readMailRequest.MailId, userId);

        if(mail == null)
        {
            _logger.ZLogInformation($"[{userId} {userName}] Failed Fetch Mail : {readMailRequest.MailId}");

            return new MailReadResponse
            {
                Error = ErrorState.NoneExistMail
            };
        }

        /* 메일 읽음 표시 진행 */
        if(!await _mailboxAccessDB.ReadMail(readMailRequest.MailId))
        {
            _logger.ZLogInformation($"[{userId} {userName}] Already Read Mail : {readMailRequest.MailId}");

            return new MailReadResponse 
            { 
                Error = ErrorState.AlreadyReadMail 
            };
        }

        MailItem? mailItem = await _mailboxAccessDB.GetMailItem(readMailRequest.MailId);

        _logger.ZLogInformation($"[{userId} {userName}] Success Read Mail {readMailRequest.MailId}");

        return new MailReadResponse
        {
            Error = ErrorState.None,
            Mail = mail,
            MailItem = mailItem
        };
    }

    async Task<ErrorState> Verify(int mailId, int userId)
    {
        if (await _mailboxAccessDB.IsDeadLine(mailId))
        {
            return ErrorState.AlreadyMailDeadlineExpireDate;
        }
        if (await _mailboxAccessDB.IsDeletedMail(mailId))
        {
            return ErrorState.DeletedMail;
        }
        if(!await _mailboxAccessDB.IsMailOwner(mailId, userId))
        {
            return ErrorState.NoneOwnerThisMail;
        }
        return ErrorState.None;
    }
}
