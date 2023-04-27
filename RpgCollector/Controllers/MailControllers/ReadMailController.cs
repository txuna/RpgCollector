using Microsoft.AspNetCore.Mvc;
using RpgCollector.Models;
using RpgCollector.RequestModels;
using RpgCollector.ResponseModels;
using RpgCollector.Services;

namespace RpgCollector.Controllers.MailControllers
{
    public class ReadMailController : Controller
    {
        IMailboxAccessDB _mailboxAccessDB;
        public ReadMailController(IMailboxAccessDB mailboxAccessDB)
        {
            _mailboxAccessDB = mailboxAccessDB;
        }

/*
 * MailId를 기반으로 해당 메일 전송 Title과 Content
 * 읽음 처리 진행 
 */
        [Route("/Mail/Read")]
        [HttpPost]
        public async Task<JsonResult> ReadMail([FromBody] ReadMailRequest readMailRequest)
        {
            Mailbox? mail = await _mailboxAccessDB.GetMail(readMailRequest.MailId);
            if(mail == null)
            {
                return Json(new FailResponse
                {
                    Success = false,
                    Message = "None Exist Mail"
                });
            }

            if(!await _mailboxAccessDB.ReadMail(readMailRequest.MailId))
            {
                return Json(new FailResponse
                {
                    Success = false,
                    Message = "Already Read Mail"
                });
            }

            ReadMailResponse mailReadResponse = new ReadMailResponse
            {
                Success = true,
                Title = mail.Title,
                Content = mail.Content
            };

            return Json(mailReadResponse);
        }
    }
}
