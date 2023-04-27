using Microsoft.AspNetCore.Mvc;
using RpgCollector.ResponseModels;
using RpgCollector.Services;

namespace RpgCollector.Controllers
{
    public class LogoutController : Controller
    {
        IAccountDB _accountDB;
        IMemoryDB _memoryDB;

        public LogoutController(IAccountDB accountDB, IMemoryDB memoryDB) 
        {
            _accountDB = accountDB;
            _memoryDB = memoryDB;
        }
        [Route("/Logout")]
        [HttpPost]
        public async Task<JsonResult> Logout()
        {
            var userName = HttpContext.Request.Headers["User-Name"];
            if(! await _memoryDB.RemoveUser(userName))
            {
                return Json(new FailResponse
                {
                    Success = false,
                    Message = "Failed Logout"
                });
            }

            return Json(new SuccessResponse
            {
                Success = true,
                Message = "Successfully Logout"
            });
        }
    }
}
