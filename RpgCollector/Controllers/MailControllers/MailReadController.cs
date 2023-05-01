using Microsoft.AspNetCore.Mvc;
using RpgCollector.Models.MailData;
using RpgCollector.RequestResponseModel.MailReadModel;
using RpgCollector.RequestResponseModel;
using RpgCollector.Services;
using ZLogger;

namespace RpgCollector.Controllers.MailControllers;

[ApiController]
public class MailReadController : Controller
{
    IMailboxAccessDB _mailboxAccessDB;
    IAccountDB _accountDB;
    ILogger<MailReadController> _logger;

    public MailReadController(IMailboxAccessDB mailboxAccessDB, IAccountDB accountDB, ILogger<MailReadController> logger)
    {
        _mailboxAccessDB = mailboxAccessDB;
        _accountDB = accountDB;
        _logger = logger;
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
        int userId = await _accountDB.GetUserId(userName);

        _logger.ZLogInformation($"[{userId} {userName}] Request 'Read Mail'");

        if(!await _mailboxAccessDB.IsMailOwner(readMailRequest.MailId, userId))
        {
            _logger.ZLogInformation($"[{userId} {userName}] None Have Permission This Mail {readMailRequest.MailId}");

            return new MailReadResponse
            {
                Error = ErrorState.NoneOwnerThisMail
            };
        }

        Mailbox? mail = await _mailboxAccessDB.GetMailFromUserId(readMailRequest.MailId, userId);

        if(mail == null)
        {
            _logger.ZLogInformation($"[{userId} {userName}] Failed Fetch Mail : {readMailRequest.MailId}");

            return new MailReadResponse
            {
                Error = ErrorState.NoneExistMail
            };
        }

        if(!await _mailboxAccessDB.ReadMail(readMailRequest.MailId))
        {
            _logger.ZLogInformation($"[{userId} {userName}] Already Read Mail : {readMailRequest.MailId}");

            return new MailReadResponse 
            { 
                Error = ErrorState.AlreadyReadMail 
            };
        }

        _logger.ZLogInformation($"[{userId} {userName}] Success Read Mail {readMailRequest.MailId}");

        return new MailReadResponse
        {
            Error = ErrorState.None,
            Title = mail.Title,
            Content = mail.Content
        };
    }
}
