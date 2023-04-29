using Microsoft.AspNetCore.Mvc;
using RpgCollector.Models;
using RpgCollector.RequestResponseModel.RegisterModel;
using RpgCollector.RequestResponseModel;
using RpgCollector.Services;
using System.Diagnostics;

namespace RpgCollector.Controllers.AuthenticateController;

[ApiController]
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
    public async Task<RegisterResponse> Register(RegisterRequest registerRequest)
    {
        int userId = await _accountDB.RegisterUser(registerRequest.UserName, registerRequest.Password);
        if (userId == -1)
        {
            return new RegisterResponse
            {
                Error = ErrorState.FailedRegister
            };
        }

        if (!await _playerAccessDB.CreatePlayer(userId))
        {
            if(!await _accountDB.UndoRegisterUser(registerRequest.UserName))
            {
                return new RegisterResponse 
                { 
                    Error = ErrorState.FailedUndoRegisterUser 
                };
            }
            return new RegisterResponse
            {
                Error = ErrorState.FailedCreatePlayer
            };
        }

        return new RegisterResponse
        {
            Error = ErrorState.None
        };
    }
}
