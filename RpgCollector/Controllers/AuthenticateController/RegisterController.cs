using Microsoft.AspNetCore.Mvc;
using RpgCollector.Models;
using RpgCollector.RequestResponseModel.RegisterModel;
using RpgCollector.RequestResponseModel;
using RpgCollector.Services;
using System.Diagnostics;
using ZLogger;

namespace RpgCollector.Controllers.AuthenticateController;

[ApiController]
public class RegisterController : Controller
{
    IAccountDB _accountDB;
    IPlayerAccessDB _playerAccessDB;
    ILogger<RegisterController> _logger;

    public RegisterController(IAccountDB accountDB, IPlayerAccessDB playerAccessDB, ILogger<RegisterController> logger)
    {
        _accountDB = accountDB;
        _playerAccessDB = playerAccessDB;
        _logger = logger;
    }

    [Route("/Register")]
    [HttpPost]
    public async Task<RegisterResponse> Register(RegisterRequest registerRequest)
    {
        int userId = await _accountDB.RegisterUser(registerRequest.UserName, registerRequest.Password);

        _logger.ZLogInformation($"[{userId}] Request 'Regist'");

        if (userId == -1)
        {
            _logger.ZLogInformation($"[{userId}] Already Exist UserName");

            return new RegisterResponse
            {
                Error = ErrorCode.AlreadyExistUser
            };
        }

        if (!await CreatePlayer(userId))
        {
            _logger.ZLogError($"[{userId}]Can not Create Player");

            ErrorCode Error = await UndoCreatePlayer(registerRequest.UserName, userId);

            return new RegisterResponse
            {
                Error = Error
            };
        }

        _logger.ZLogInformation($"[{userId}] Success Register and Create Player in Database");

        return new RegisterResponse
        {
            Error = ErrorCode.None
        };
    }

    async Task<bool> CreatePlayer(int userId)
    {
        if (!await _playerAccessDB.SetInitPlayerState(userId))
        {
            return false;
        }
        if (!await _playerAccessDB.SetInitPlayerItems(userId))
        {
            return false;
        }

        return true;
    }

    async Task<ErrorCode> UndoCreatePlayer(string userName, int userId)
    {
        if (!await _accountDB.UndoRegisterUser(userName))
        {
            _logger.ZLogError($"[{userId}] Can not undo register account");

            return ErrorCode.FailedUndoRegisterUser;
        }

        if (!await _playerAccessDB.UndoCreatePlayer(userId))
        {
            _logger.ZLogError($"[{userId}] Can not undo create player");

            return ErrorCode.FailedUndoRegisterUser;
        }

        return ErrorCode.FailedCreatePlayer;
    }
}
