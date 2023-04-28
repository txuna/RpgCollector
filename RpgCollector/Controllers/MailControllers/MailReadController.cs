using Microsoft.AspNetCore.Mvc;
using RpgCollector.Models.MailData;
using RpgCollector.RequestModels.MailRequest;
using RpgCollector.ResponseModels;
using RpgCollector.ResponseModels.MailResponse;
using RpgCollector.Services;

namespace RpgCollector.Controllers.MailControllers
{
    public class MailReadController : Controller
    {
        IMailboxAccessDB _mailboxAccessDB;
        public MailReadController(IMailboxAccessDB mailboxAccessDB)
        {
            _mailboxAccessDB = mailboxAccessDB;
        }

/*
 * MailId를 기반으로 해당 메일 전송 Title과 Content
 * 읽음 처리 진행 
 */
        [Route("/Mail/Read")]
        [HttpPost]
        public async Task<JsonResult> ReadMail([FromBody] MailReadRequest readMailRequest)
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

            MailReadResponse mailReadResponse = new MailReadResponse
            {
                Success = true,
                Title = mail.Title,
                Content = mail.Content
            };

            return Json(mailReadResponse);
        }
    }
}
