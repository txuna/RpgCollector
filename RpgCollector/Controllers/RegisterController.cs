using Microsoft.AspNetCore.Mvc;
using RpgCollector.Models;
using RpgCollector.RequestModels;
using RpgCollector.ResponseModels;
using RpgCollector.Services;

namespace RpgCollector.Controllers
{
    public class RegisterController : Controller
    {
        IAccountDB _accountDB;
        IPlayerAccessDB _playerAccessDB;

        public RegisterController(IAccountDB accountDB, IPlayerAccessDB playerAccessDB)
        {
            _accountDB = accountDB;
            _playerAccessDB = playerAccessDB;
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

            User user = await _accountDB.GetUser(userRequest.UserName);

            if(!await _playerAccessDB.CreatePlayer(user.UserId))
            {
                return Json(new FailResponse
                {
                    Success = false,
                    Message = "Failed Created Player"
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
