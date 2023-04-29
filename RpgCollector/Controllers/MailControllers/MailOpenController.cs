using Microsoft.AspNetCore.Mvc;
using RpgCollector.Models;
using RpgCollector.Models.MailData;
using RpgCollector.RequestResponseModel.MailOpenModel;
using RpgCollector.RequestResponseModel;
using RpgCollector.Services;

namespace RpgCollector.Controllers.MailControllers;

/*
 * 처음열었는지에 대한 부분과 
 * 다음페이지를 보기 위한부분 Request에 추가
 */
[ApiController]
public class MailOpenController : Controller
{
    IMailboxAccessDB _mailboxAccessDB;
    IAccountDB _accountDB;

    public MailOpenController(IMailboxAccessDB mailboxAccessDB, IAccountDB accountDB)
    {
        _mailboxAccessDB = mailboxAccessDB;
        _accountDB = accountDB;
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
        int userId = await _accountDB.GetUserId(userName);
        
        if (userId == -1)
        {
            return new MailOpenResponse
            {
                Error = ErrorState.FailedConnectRedis
            };
        }

        // ReceiverId에 맞게 가지고 오기
        Mailbox[]? mails = await _mailboxAccessDB.GetAllMailFromUserId(userId);

        if(mails == null)
        {
            return new MailOpenResponse
            {
                Error = ErrorState.FailedFetchMail
            };
        }
        MailOpenResponse? mailOpenResponse = getPartialMails(mails, openMailboxRequest);

        if(mailOpenResponse == null)
        {
            return new MailOpenResponse
            {
                Error = ErrorState.InvalidPageNumber
            };
        }

        return mailOpenResponse;
    }

    MailOpenResponse? getPartialMails(Mailbox[] mails, MailOpenRequest openMailboxRequest)
    {
        if(mails.Length == 0)
        {
            return new MailOpenResponse
            {
                Error = ErrorState.None,
                Mails = mails
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
