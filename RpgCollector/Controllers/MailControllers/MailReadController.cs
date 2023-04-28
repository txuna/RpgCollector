using Microsoft.AspNetCore.Mvc;
using RpgCollector.Models.MailData;
using RpgCollector.RequestResponseModel.MailReadModel;
using RpgCollector.RequestResponseModel;
using RpgCollector.Services;

namespace RpgCollector.Controllers.MailControllers
{
    public class MailReadController : Controller
    {
        IMailboxAccessDB _mailboxAccessDB;
        IAccountDB _accountDB;

        public MailReadController(IMailboxAccessDB mailboxAccessDB, IAccountDB accountDB)
        {
            _mailboxAccessDB = mailboxAccessDB;
            _accountDB = accountDB;
        }

        /*
         * MailId를 기반으로 해당 메일 전송 Title과 Content
         * 읽음 처리 진행 
         */
        [Route("/Mail/Read")]
        [HttpPost]
        public async Task<MailReadResponse> ReadMail([FromBody] MailReadRequest readMailRequest)
        {
            if (!ModelState.IsValid)
            {
                return new MailReadResponse
                {
                    Error = ErrorState.InvalidModel
                };
            }

            string userName = HttpContext.Response.Headers["User-Name"]; 
            int userId = await _accountDB.GetUserId(userName);

            if(!await _mailboxAccessDB.IsMailOwner(readMailRequest.MailId, userId))
            {
                return new MailReadResponse
                {
                    Error = ErrorState.NoneOwnerThisMail
                };
            }

            Mailbox? mail = await _mailboxAccessDB.GetMailFromUserId(readMailRequest.MailId, userId);

            if(mail == null)
            {
                return new MailReadResponse
                {
                    Error = ErrorState.NoneExistMail
                };
            }

            if(!await _mailboxAccessDB.ReadMail(readMailRequest.MailId))
            {
                return new MailReadResponse 
                { 
                    Error = ErrorState.AlreadyReadMail 
                };
            }

            return new MailReadResponse
            {
                Error = ErrorState.None,
                Title = mail.Title,
                Content = mail.Content
            };
        }
    }
}
