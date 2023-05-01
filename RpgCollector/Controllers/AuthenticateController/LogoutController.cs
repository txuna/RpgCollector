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
        if(userId == -1)
        {
            _logger.ZLogError($"Cannot Found UserName : {userName}");
        }

        if (!await _accountMemoryDB.RemoveUser(userName))
        {
            _logger.ZLogError($"Failed Remove User in Redis UserId : {userId} UserName : {userName}");
            return new LogoutResponse
            {
                Error = ErrorState.FailedConnectRedis
            };
        }

        _logger.ZLogInformation($"Success Logout User in Redis UserId : {userId} UserName : {userName}");

        return new LogoutResponse
        {
            Error = ErrorState.None
        };
    }
}
