﻿using Microsoft.AspNetCore.Mvc;
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
    IAccountMemoryDB _accountMemoryDB;
    ILogger<LogoutController> _logger;

    public LogoutController(IAccountMemoryDB accountMemoryDB, ILogger<LogoutController> logger)
    {
        _accountMemoryDB = accountMemoryDB;
        _logger = logger;
    }

    [Route("/Logout")]
    [HttpPost]
    public async Task<LogoutResponse> Logout()
    {
        string userName = HttpContext.Request.Headers["User-Name"];
        int userId = Convert.ToInt32(HttpContext.Items["User-Id"]);

        _logger.ZLogInformation($"[{userId}] Request /Logout");

        if (await _accountMemoryDB.RemoveUser(userName) == false)
        {
            _logger.ZLogError($"[{userId}] Failed Remove User in Redis");
            return new LogoutResponse
            {
                Error = ErrorCode.FailedConnectRedis
            };
        }

        _logger.ZLogInformation($"[{userId}] Success Logout User in Redis UserId");

        return new LogoutResponse
        {
            Error = ErrorCode.None
        };
    }
}
