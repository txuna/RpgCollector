using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using RpgCollector.Models.AccountModel;
using RpgCollector.RequestResponseModel;
using RpgCollector.RequestResponseModel.LoginModel;
using RpgCollector.Services;
using RpgCollector.Utility;
using System.Text.Json;
using ZLogger;

namespace RpgCollector.Controllers.AuthenticateController;

[ApiController]
public class LoginController : Controller
{
    IAccountDB _accountDB;
    IAccountMemoryDB _memoryDB;
    ILogger<LoginController> _logger;

    public LoginController(IAccountDB accountDB, IAccountMemoryDB memoryDB, ILogger<LoginController> logger)
    {
        _accountDB = accountDB;
        _memoryDB = memoryDB;
        _logger = logger;
    }
    /*
     * 로그인 요청 컨트롤러
     * 1. 모델 바인딩시 유효성 판단 
     * 2. 디비 서비스에 로그인 로직 요청
     * 3. 성공적이라면 content에 마스터 데이터 + 유저 데이터 전송 - redirect로
     */
    [Route("/Login")]
    [HttpPost]
    public async Task<LoginResponse> Login(LoginRequest loginRequest)
    {
        User? user = await _accountDB.GetUser(loginRequest.UserName);

        _logger.ZLogInformation($"[{loginRequest.UserName}] Request /Login");

        if (user == null)
        {
            _logger.ZLogInformation($"[{loginRequest.UserName}] None Exist UserName");
            return new LoginResponse
            {
                Error = ErrorCode.NoneExistName,
            };
        }

        if(!VerifyPassword(user, loginRequest.Password))
        {
            _logger.ZLogError($"[{user.UserId} {loginRequest.UserName}] Invalid Password");

            return new LoginResponse
            {
                Error = ErrorCode.InvalidPassword,
            };
        }
       
        string authToken = HashManager.GenerateAuthToken();
        authToken = authToken.Replace("+", "d");

        ErrorCode Error = await StoreUserInMemory(user, authToken);
        
        if(Error != ErrorCode.None)
        {
            _logger.ZLogInformation($"[{user.UserId} {loginRequest.UserName}] Failed Login");

            return new LoginResponse
            {
                Error = Error
            };
        }

        _logger.ZLogInformation($"[{user.UserId} {loginRequest.UserName}] Complement Login");

        return new LoginResponse
        {
            Error = ErrorCode.None, 
            UserName = user.UserName,
            AuthToken = authToken
        };
    }

    async Task<ErrorCode> StoreUserInMemory(User user, string authToken)
    {
        if (await _memoryDB.StoreUser(user, authToken) == false)
        {
            return ErrorCode.FailedConnectRedis;
        }

        return ErrorCode.None;
    }

    bool VerifyPassword(User user, string requestPassword)
    {
        if (user.Password != HashManager.GenerateHash(requestPassword, user.PasswordSalt))
        {
            return false;
        }

        return true;
    }

}
