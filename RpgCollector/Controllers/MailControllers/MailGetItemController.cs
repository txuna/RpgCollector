using Microsoft.AspNetCore.Mvc;
using RpgCollector.Models;
using RpgCollector.Models.MailData;
using RpgCollector.RequestModels.MailRequest;
using RpgCollector.ResponseModels;
using RpgCollector.Services;

namespace RpgCollector.Controllers.MailControllers
{
    public class MailGetItemController : Controller
    {
        IMailboxAccessDB _mailboxAccessDB;
        IPlayerAccessDB _playerAccessDB;
        IAccountMemoryDB _accountMemoryDB;

        public MailGetItemController(IMailboxAccessDB mailboxAccessDB, IPlayerAccessDB playerAccessDB, IAccountMemoryDB accountMemoryDB) 
        {
            _mailboxAccessDB = mailboxAccessDB; 
            _playerAccessDB = playerAccessDB;
            _accountMemoryDB = accountMemoryDB;
        }
/*
 * 사용자가 전송한 mailId를 기반으로 아이템을 동봉하고 있는지 확인 + 이미 수령했는지 확인 및 아이템 제공
 * 사용자의 인벤토리 업데이트
 */
        [Route("/Mail/Item")]
        [HttpPost]
        public async Task<JsonResult> GetItem([FromBody] MailReadRequest readMailRequest)
        {
            if(!await _mailboxAccessDB.HasMailItem(readMailRequest.MailId))
            {
                return Json(new FailResponse
                {
                    Success = false, 
                    Message = "This Mail Didn't Have Item"
                });
            }

            MailItem? mailItem = await _mailboxAccessDB.ReceiveMailItem(readMailRequest.MailId);

            if(mailItem == null)
            {
                return Json(new FailResponse
                {
                    Success = false,
                    Message = "Already Received Item From Mail"
                });
            }

            var userName = HttpContext.Request.Headers["User-Name"];
            RedisUser? redisUser = await _accountMemoryDB.GetUserFromName(userName);

            if (redisUser == null)
            {
                return Json(new FailResponse
                {
                    Success = false,
                    Message = "Failed Fetch Redis User"
                });
            }

            if (!await _playerAccessDB.AddItemToPlayer(redisUser.UserId, mailItem.itemId, mailItem.Quantity))
            {
                if (!await _mailboxAccessDB.UndoMailItem(mailItem.itemId))
                {
                    return Json(new FailResponse
                    {
                        Success = false,
                        Message = "Failed Undo Mail Item. So Sorry...!. Please Contact to Administrator"
                    });
                }

                return Json(new FailResponse
                {
                    Success = false,
                    Message = "Failed Add Item into the Player from Mail, Undo Rollback Mail Item"
                });
            }

            return Json(new SuccessResponse
            {
                Success = true,
                Message = "Successfully Received Item From Mail"
            });
        }
    }
}
