using Microsoft.AspNetCore.Mvc;
using RpgCollector.Models;
using RpgCollector.RequestResponseModel;
using RpgCollector.RequestResponseModel.LogoutModel;
using RpgCollector.RequestResponseModel.RegisterModel;
using RpgCollector.Services;
using ZLogger;

namespace RpgCollector.Controllers.AuthenticateController;

[ApiController]
public class LogoutController : Controller
{
    IAccountDB _accountDB;
    IAccountMemoryDB _accountMemoryDB;
    ILogger<LogoutController> _logger;

    public LogoutController(IAccountDB accountDB, IAccountMemoryDB accountMemoryDB, ILogger<LogoutController> logger)
    {
        _accountMemoryDB = accountMemoryDB;
        _logger = logger;
        _accountDB = accountDB;
    }

    [Route("/Logout")]
    [HttpPost]
    public async Task<LogoutResponse> Logout()
    {
        string userName = HttpContext.Request.Headers["User-Name"];
        int userId = await _accountDB.GetUserId(userName);

        _logger.ZLogInformation($"[{userId} {userName}] Request 'Logout'");

        if (userId == -1)
        {
            _logger.ZLogError($"[{userName}] Cannot Found in Redis");
        }

        if (!await _accountMemoryDB.RemoveUser(userName))
        {
            _logger.ZLogError($"[{userId} {userName}] Failed Remove User in Redis");
            return new LogoutResponse
            {
                Error = ErrorState.FailedConnectRedis
            };
        }

        _logger.ZLogInformation($"[{userId} {userName}] Success Logout User in Redis UserId");

        return new LogoutResponse
        {
            Error = ErrorState.None
        };
    }
}
