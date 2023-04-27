using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using RpgCollector.Models;
using RpgCollector.RequestModels;
using RpgCollector.ResponseModels;
using RpgCollector.Services;
using RpgCollector.Utility;
using System.Text.Json;

namespace RpgCollector.Controllers.AuthenticateController
{
    public class LoginController : Controller
    {
        IAccountDB _accountDB;
        IAccountMemoryDB _memoryDB;

        public LoginController(IAccountDB accountDB, IAccountMemoryDB memoryDB)
        {
            _accountDB = accountDB;
            _memoryDB = memoryDB;
        }
        /*
         * 로그인 요청 컨트롤러
         * 1. 모델 바인딩시 유효성 판단 
         * 2. 디비 서비스에 로그인 로직 요청
         * 3. 성공적이라면 content에 마스터 데이터 + 유저 데이터 전송 - redirect로
         */
        [Route("/Login")]
        [HttpPost]
        public async Task<JsonResult> Login([FromBody] UserRequest userRequest)
        {
            if (!ModelState.IsValid)
            {
                return Json(new FailResponse
                {
                    Success = false,
                    Message = "Invalid Model"
                });
            }

            User? user = await _accountDB.GetUser(userRequest.UserName);

            if (user == null)
            {
                return Json(new FailResponse
                {
                    Success = false,
                    Message = "None Exist User"
                });
            }

            // 패스워드 확인
            if (!_accountDB.VerifyPassword(user, userRequest.Password))
            {
                return Json(new FailResponse
                {
                    Success = false,
                    Message = "Invalid Password"
                });
            }
            // 중복 인증 확인 
            if (!await _memoryDB.IsDuplicateLogin(userRequest.UserName))
            {
                return Json(new FailResponse
                {
                    Success = false,
                    Message = "Duplicated Login"
                });
            }
            // 로그인에 성공한 User에게 Token 발급 && // 유니코드 변환 문제
            string authToken = HashManager.GenerateAuthToken();
            authToken = authToken.Replace("+", "d");

            if (!await _memoryDB.StoreUser(user, authToken))
            {
                return Json(new FailResponse
                {
                    Success = false,
                    Message = "Failed Store Account in Redis"
                });
            }

            return Json(new LoginResponse
            {
                Success = true,
                UserName = user.UserName,
                AuthToken = authToken,
            });
        }


    }
}
