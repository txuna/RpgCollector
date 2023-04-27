using Microsoft.AspNetCore.Mvc;
using RpgCollector.RequestModels;
using RpgCollector.ResponseModels;
using RpgCollector.Services;

namespace RpgCollector.Controllers
{
    public class RegisterController : Controller
    {
        IAccountDB _accountDB;
        IMemoryDB _memoryDB;

        public RegisterController(IAccountDB accountDB, IMemoryDB memoryDB)
        {
            _accountDB = accountDB;
            _memoryDB = memoryDB;
        }

        [Route("/Register")]
        [HttpPost]
        public async Task<JsonResult> Register([FromBody] UserRequest userRequest)
        {
            if (!ModelState.IsValid)
            {
                return Json(new FailResponse
                {
                    Success = false,
                    Message = "Invalid Model"
                });
            }
            if(!await _accountDB.RegisterUser(userRequest.UserName, userRequest.Password))
            {
                return Json(new FailResponse
                {
                    Success = false,
                    Message = "Failed Register"
                });
            }
            return Json(new SuccessResponse
            {
                Success = true,
                Message = "Successfully Register"
            });
        }
    }
}
