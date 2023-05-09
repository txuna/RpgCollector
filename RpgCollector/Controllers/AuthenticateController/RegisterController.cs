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

        _logger.ZLogInformation($"[{userId} {registerRequest.UserName}] Request 'Regist'");

        if (userId == -1)
        {
            _logger.ZLogInformation($"[{registerRequest.UserName}] Already Exist UserName");

            return new RegisterResponse
            {
                Error = ErrorState.AlreadyExistUser
            };
        }

        if (!await CreatePlayer(userId))
        {
            _logger.ZLogError($"[{userId} {registerRequest.UserName}]Can not Create Player");
            _logger.ZLogInformation($"[{userId} {registerRequest.UserName}] Trying... Undo Register Account");

            if (!await _accountDB.UndoRegisterUser(registerRequest.UserName))
            {
                _logger.ZLogError($"[{userId} {registerRequest.UserName}] Can not undo register account");

                return new RegisterResponse 
                { 
                    Error = ErrorState.FailedUndoRegisterUser 
                };
            }
            _logger.ZLogError($"[{userId} {registerRequest.UserName}] Success Undo(remove) Player in Account DB");

            return new RegisterResponse
            {
                Error = ErrorState.FailedCreatePlayer
            };
        }

        _logger.ZLogInformation($"[{userId} {registerRequest.UserName}] Success Register and Create Player in Database");

        return new RegisterResponse
        {
            Error = ErrorState.None
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
}
