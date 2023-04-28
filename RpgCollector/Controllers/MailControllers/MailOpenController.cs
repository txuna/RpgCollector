using Microsoft.AspNetCore.Mvc;
using RpgCollector.Models;
using RpgCollector.Models.MailData;
using RpgCollector.RequestModels.MailRequest;
using RpgCollector.ResponseModels;
using RpgCollector.ResponseModels.MailResponse;
using RpgCollector.Services;

namespace RpgCollector.Controllers.MailControllers
{
    /*
     * 처음열었는지에 대한 부분과 
     * 다음페이지를 보기 위한부분 Request에 추가
     */
    public class MailOpenController : Controller
    {
        IMailboxAccessDB _mailboxAccessDB;
        IAccountMemoryDB _accountMemoryDB;

        public MailOpenController(IMailboxAccessDB mailboxAccessDB, IAccountMemoryDB accountMemoryDB)
        {
            _mailboxAccessDB = mailboxAccessDB;
            _accountMemoryDB = accountMemoryDB;
        }

/*
*  우편함을 처음 오픈하는것이라면 전채갯수/20해서 나온 TotalPageNumber 하고 날짜별로 정렬된 상위 20개만 전송 
*  그외에는 요청받은 PageNumber에 따라서 반환
*/
        [Route("/Mail/Open")]
        [HttpPost]
        public async Task<JsonResult> OpenMailbox([FromBody] MailOpenRequest openMailboxRequest)
        {
            if(!ModelState.IsValid)
            {
                return Json(new FailResponse
                {
                    Success = false,
                    Message = "Invalid Model"
                }); 
            }

            var userName = HttpContext.Request.Headers["User-Name"];
            RedisUser? redisUser = await _accountMemoryDB.GetUserFromName(userName);

            if(redisUser == null)
            {
                return Json(new FailResponse
                {
                    Success = false,
                    Message = "Failed Fetch Redis User"
                });
            }

            // ReceiverId에 맞게 가지고 오기
            Mailbox[]? mails = await _mailboxAccessDB.GetAllMailFromUserId(redisUser.UserId);

            if(mails == null)
            {
                return Json(new FailResponse
                {
                    Success = false,
                    Message = "Failed Fetch Mail"
                });
            }

            MailboxResponse? mailboxResponse = getPartialMails(mails, openMailboxRequest);

            if(mailboxResponse == null)
            {
                return Json(new FailResponse
                {
                    Success = false,
                    Message = "Invalid Page Number"
                });
            }

            return Json(mailboxResponse);
        }

        public MailboxResponse? getPartialMails(Mailbox[] mails, MailOpenRequest openMailboxRequest)
        {
            int totalPageNumber = (int)Math.Ceiling((double)mails.Length / 20.0);
            Mailbox[] partialMail;

            if (openMailboxRequest.IsFirstOpen == true)
            {
                partialMail = new Mailbox[20];
                Array.Copy(mails, 0, partialMail, 0, 20);
            }
            else
            {
                if (openMailboxRequest.PageNumber > totalPageNumber)
                {
                    return null;
                }

                int start = (openMailboxRequest.PageNumber - 1) * 20;
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

            MailboxResponse mailboxResponse = new MailboxResponse
            {
                Success = true,
                TotalPageNumber = totalPageNumber,
                Mails = partialMail
            };

            return mailboxResponse;
        }
    }
}
