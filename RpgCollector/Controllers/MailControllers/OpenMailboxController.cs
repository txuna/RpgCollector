using Microsoft.AspNetCore.Mvc;
using RpgCollector.Models;
using RpgCollector.RequestModels;
using RpgCollector.ResponseModels;
using RpgCollector.Services;

namespace RpgCollector.Controllers.MailControllers
{
    /*
     * 처음열었는지에 대한 부분과 
     * 다음페이지를 보기 위한부분 Request에 추가
     */
    public class OpenMailboxController : Controller
    {
        IMailboxAccessDB _mailboxAccessDB;
        IAccountMemoryDB _accountMemoryDB;

        public OpenMailboxController(IMailboxAccessDB mailboxAccessDB, IAccountMemoryDB accountMemoryDB)
        {
            _mailboxAccessDB = mailboxAccessDB;
            _accountMemoryDB = accountMemoryDB;
        }

/*
*  우편함을 처음 오픈하는것이라면 전채갯수/20해서 나온 TotalPageNumber 하고 날짜별로 정렬된 상위 20개만 전송 
*  그외에는 요청받은 PageNumber에 따라서 반환
*/
        [Route("/Game/Mail")]
        [HttpPost]
        public async Task<JsonResult> OpenMailbox([FromBody] OpenMailboxRequest openMailboxRequest)
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
            RedisUser redisUser = await _accountMemoryDB.GetUserFromName(userName);

            if(redisUser == null)
            {
                return Json(new FailResponse
                {
                    Success = false,
                    Message = "Failed Fetch Redis User"
                });
            }

            // ReceiverId에 맞게 가지고 오기
            Mailbox[] mails = await _mailboxAccessDB.GetAllMailFromUserId(redisUser.UserId);

            if(mails == null)
            {
                return Json(new FailResponse
                {
                    Success = false,
                    Message = "Failed Fetch Mail"
                });
            }

            int totalPageNumber = (int)Math.Ceiling((double)mails.Length / 20.0);
            Mailbox[] partialMail = new Mailbox[20];

            if (openMailboxRequest.IsFirstOpen == true)
            {
                Array.Copy(mails, 0, partialMail, 0, 20);
            }
            else
            {
                if(openMailboxRequest.PageNumber > totalPageNumber)
                {
                    return Json(new FailResponse
                    {
                        Success = false,
                        Message = "Invalid Page Number"
                    });
                }
                Array.Copy(mails, (openMailboxRequest.PageNumber - 1) * 20, partialMail, 0, 20);
            }

            return Json(new MailboxResponse
            {
                Success = true,
                TotlaPageNumber = totalPageNumber,
                Mails = partialMail
            });
        }
    }
}
