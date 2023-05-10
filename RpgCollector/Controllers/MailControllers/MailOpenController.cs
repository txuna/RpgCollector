using Microsoft.AspNetCore.Mvc;
using RpgCollector.Models;
using RpgCollector.Models.MailModel;
using RpgCollector.RequestResponseModel.MailOpenModel;
using RpgCollector.RequestResponseModel;
using RpgCollector.Services;
using ZLogger;
using RpgCollector.RequestResponseModel.MailReadModel;

namespace RpgCollector.Controllers.MailControllers;

/*
 * 처음열었는지에 대한 부분과 
 * 다음페이지를 보기 위한부분 Request에 추가
 */
[ApiController]
public class MailOpenController : Controller
{
    IMailboxAccessDB _mailboxAccessDB;
    ILogger<MailOpenController> _logger;

    public MailOpenController(IMailboxAccessDB mailboxAccessDB, 
                              ILogger<MailOpenController> logger)
    {
        _mailboxAccessDB = mailboxAccessDB;
        _logger = logger;
    }

    [Route("/Mail/Open")]
    [HttpPost]
    public async Task<MailOpenResponse> OpenMailbox(MailOpenRequest openMailboxRequest)
    {
        int userId = Convert.ToInt32(HttpContext.Items["User-Id"]);

        /* userId가 receiverdId인 모든 메일 20개만 가지고옴 */
        Mailbox[]? mails = await _mailboxAccessDB.GetPartialMails(userId, (int)openMailboxRequest.PageNumber);

        if (mails == null)
        {
            _logger.ZLogInformation($"[{userId}] Invalid PageNumber");

            return new MailOpenResponse
            {
                Error = ErrorState.InvalidPageNumber
            };
        }

        int totalPageNumber = await _mailboxAccessDB.GetTotalMailNumber(userId);

        return new MailOpenResponse
        {
            Error = ErrorState.None,
            Mails = ProcessingMail(mails),
            TotalPageNumber = (int)Math.Ceiling((double)totalPageNumber / 20)
        };
    }

    OpenMail[] ProcessingMail(Mailbox[] mails)
    {
        OpenMail[] openMail = new OpenMail[mails.Length];

        for(int i = 0; i < mails.Length; i++)
        {
            openMail[i] = new OpenMail();
            openMail[i].Title = mails[i].Title;
            openMail[i].MailId = mails[i].MailId;
        }

        return openMail;
    }
}
