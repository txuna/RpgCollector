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
    ILogger _logger;

    public RegisterController(IAccountDB accountDB, IPlayerAccessDB playerAccessDB, ILogger logger)
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

        if (userId == -1)
        {
            _logger.ZLogInformation($"Already Exist UserName : {registerRequest.UserName}");
            return new RegisterResponse
            {
                Error = ErrorState.FailedRegister
            };
        }

        if (!await CreatePlayer(userId))
        {
            _logger.ZLogError($"Can not Create Player UserId : {userId} UserName : {registerRequest.UserName}");
            _logger.ZLogInformation($"Trying... Undo Register Account UserId : {userId} UserName : {registerRequest.UserName}");
            if (!await _accountDB.UndoRegisterUser(registerRequest.UserName))
            {
                _logger.ZLogError($"Can not undo register account UserId : {userId} UserName : {registerRequest.UserName}");
                return new RegisterResponse 
                { 
                    Error = ErrorState.FailedUndoRegisterUser 
                };
            }
            _logger.ZLogError($"Success Undo(remove) Player in Account DB UserId : {userId} UserName : {registerRequest.UserName}");
            return new RegisterResponse
            {
                Error = ErrorState.FailedCreatePlayer
            };
        }

        _logger.ZLogInformation($"Success Register and Create Player in Database UserId : {userId} UserName : {registerRequest.UserName}");

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
