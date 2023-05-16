using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using RpgCollector.Models.AccountModel;
using RpgCollector.Models.StageModel;
using RpgCollector.RequestResponseModel;
using RpgCollector.RequestResponseModel.AccountReqRes;
using RpgCollector.Services;
using RpgCollector.Utility;
using System.Text.Json;
using ZLogger;

namespace RpgCollector.Controllers.AuthenticateController;

[ApiController]
public class LoginController : Controller
{
    IAccountDB _accountDB;
    IRedisMemoryDB _memoryDB;
    ILogger<LoginController> _logger;

    public LoginController(IAccountDB accountDB, IRedisMemoryDB memoryDB, ILogger<LoginController> logger)
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

        _logger.ZLogDebug($"[{user.UserName}] Request /Login");

        if (user == null)
        {
            _logger.ZLogDebug($"[{loginRequest.UserName}] None Exist UserName");
            return new LoginResponse
            {
                Error = ErrorCode.NoneExistName,
            };
        }

        if(!VerifyPassword(user, loginRequest.Password))
        {
            _logger.ZLogDebug($"[{user.UserId} {loginRequest.UserName}] Invalid Password");

            return new LoginResponse
            {
                Error = ErrorCode.InvalidPassword,
            };
        }
       
        string authToken = HashManager.GenerateAuthToken();
        ErrorCode Error;
        UserState state; 

        // 기존 정보가 있다면 playing이였는지 확인
        if (await IsPlayingGame(user.UserName) == true)
        {
            state = UserState.Playing;
        }
        else
        {
            state = UserState.Login;
        }

        Error = await StoreUserInMemory(user, authToken, state);

        if (Error != ErrorCode.None)
        {
            _logger.ZLogDebug($"[{user.UserId} {loginRequest.UserName}] Failed Login");

            return new LoginResponse
            {
                Error = Error
            };
        }

        _logger.ZLogDebug($"[{user.UserId} {loginRequest.UserName}] Complement Login");

        return new LoginResponse
        {
            Error = ErrorCode.None, 
            UserName = user.UserName,
            AuthToken = authToken,
            State = state
        };
    }

    async Task<ErrorCode> StoreUserInMemory(User user, string authToken, UserState state)
    {
        if (await _memoryDB.StoreUser(user.UserName, user.UserId, authToken, state) == false)
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

    async Task<bool> IsPlayingGame(string userName)
    {
        RedisUser? user = await _memoryDB.GetUser(userName);
        if (user == null)
        {
            return false;
        }

        if (user.State != UserState.Playing)
        {
            return false;
        }

        // 플레이어 상태는 PLAYING이지만 던전 스테이지 정보가 없을 경우에는 PLAYING이 아닌 LOING으로 설정
        RedisPlayerStageInfo? redisPlayerStageInfo = await _memoryDB.GetRedisPlayerStageInfo(userName);
        if(redisPlayerStageInfo == null)
        {
            return false;
        }

        return true;
    }

}
