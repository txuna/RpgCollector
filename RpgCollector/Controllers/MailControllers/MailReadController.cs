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
    ILogger<MailReadController> _logger;

    public MailReadController(IMailboxAccessDB mailboxAccessDB, 
                              ILogger<MailReadController> logger)
    {
        _mailboxAccessDB = mailboxAccessDB;
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
        int userId = Convert.ToInt32(HttpContext.Items["User-Id"]);

        Mailbox? mail = await _mailboxAccessDB.GetMailFromUserId(readMailRequest.MailId, userId);

        if(mail == null)
        {
            _logger.ZLogInformation($"[{userId}] Failed Fetch Mail : {readMailRequest.MailId}");

            return new MailReadResponse
            {
                Error = ErrorCode.NoneExistMail
            };
        }

        if(!await _mailboxAccessDB.UpdateReadFlagInMail(readMailRequest.MailId))
        {
            return new MailReadResponse 
            { 
                Error = ErrorCode.FailedReadMail
            };
        }

        _logger.ZLogInformation($"[{userId}] Success Read Mail {readMailRequest.MailId}");

        return new MailReadResponse
        {
            Error = ErrorCode.None,
            MailId = mail.MailId, 
            Title = mail.Title, 
            Content = mail.Content,
            SendDate = mail.SendDate,
            ItemId = mail.ItemId,
            Quantity = mail.Quantity,
            HasReceived = mail.HasReceived,
        };
    }
}
