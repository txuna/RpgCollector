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
    IAccountMemoryDB _accountMemoryDB;
    ILogger<MailOpenController> _logger;

    public MailOpenController(IMailboxAccessDB mailboxAccessDB, 
                              IAccountMemoryDB accountMemoryDB,
                              ILogger<MailOpenController> logger)
    {
        _mailboxAccessDB = mailboxAccessDB;
        _accountMemoryDB = accountMemoryDB;
        _logger = logger;
    }

    /*
    *  우편함을 처음 오픈하는것이라면 전채갯수/20해서 나온 TotalPageNumber 하고 날짜별로 정렬된 상위 20개만 전송 
    *  그외에는 요청받은 PageNumber에 따라서 반환
    */
    [Route("/Mail/Open")]
    [HttpPost]
    public async Task<MailOpenResponse> OpenMailbox(MailOpenRequest openMailboxRequest)
    {
        var userName = HttpContext.Request.Headers["User-Name"];
        /* userName을 기반으로 userId를 불러옴 */
        int userId = await _accountMemoryDB.GetUserId(userName);

        _logger.ZLogInformation($"[{userId} {userName}] Request 'Open Mail'");

        if (userId == -1)
        {
            _logger.ZLogError($"[{userName}] Failed Connected Redis");
            return new MailOpenResponse
            {
                Error = ErrorState.FailedConnectRedis
            };
        }

        /* userId가 receiverdId인 모든 메일 가지고옴 */
        //Mailbox[]? mails = await _mailboxAccessDB.GetAllMailFromUserId(userId);

        Mailbox[]? mails = await _mailboxAccessDB.GetPartialMails(userId, (bool)openMailboxRequest.IsFirstOpen, (int)openMailboxRequest.PageNumber);

        if (mails == null)
        {
            _logger.ZLogError($"[{userId} {userName}] Invalid PageNumber");
            return new MailOpenResponse
            {
                Error = ErrorState.InvalidPageNumber
            };
        }

        /* 전체적으로 불러온 - 네트워크 과부하 */
        //MailOpenResponse? mailOpenResponse = getPartialMails(mails, openMailboxRequest);

        //if(mailOpenResponse == null)
        //{
        //    _logger.ZLogInformation($"[{userId} {userName}] Invalid PageNumber : {openMailboxRequest.PageNumber}");
        //    return new MailOpenResponse
        //    {
        //        Error = ErrorState.InvalidPageNumber
        //    };
        //}

        return new MailOpenResponse
        {
            Error = ErrorState.None,
            Mails = mails
        };
    }

    MailOpenResponse? getPartialMails(Mailbox[] mails, MailOpenRequest openMailboxRequest)
    {
        if(mails.Length == 0 || openMailboxRequest.PageNumber <= 0)
        {
            return new MailOpenResponse
            {
                Error = ErrorState.None,
                Mails = null
            };
        }

        int totalPageNumber = (int)Math.Ceiling((double)mails.Length / 20.0);
        Mailbox[] partialMail;

        if (openMailboxRequest.IsFirstOpen == true)
        {
            if(mails.Length < 20)
            {
                partialMail = new Mailbox[mails.Length];
                Array.Copy(mails, 0, partialMail, 0, mails.Length);
            }
            else
            {
                partialMail = new Mailbox[20];
                Array.Copy(mails, 0, partialMail, 0, 20);
            }
        }
        else
        {
            if (openMailboxRequest.PageNumber > totalPageNumber)
            {
                return null;
            }

            int start = ((int)openMailboxRequest.PageNumber - 1) * 20;
            int end = 20;

            if (start + 20 > mails.Length)
            {
                partialMail = new Mailbox[mails.Length - start];
                end = mails.Length - start;
            }
            else
            {
                partialMail = new Mailbox[20];
            }

            Array.Copy(mails, start, partialMail, 0, end);
        }

        return new MailOpenResponse
        {
            Error = ErrorState.None,
            Mails = partialMail
        };
    }
}
