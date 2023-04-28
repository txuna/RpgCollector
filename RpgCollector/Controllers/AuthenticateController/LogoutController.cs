using Microsoft.AspNetCore.Mvc;
using RpgCollector.RequestResponseModel;
using RpgCollector.RequestResponseModel.LogoutModel;
using RpgCollector.Services;

namespace RpgCollector.Controllers.AuthenticateController;

[ApiController]
public class LogoutController : Controller
{
    IAccountMemoryDB _accountMemoryDB;

    public LogoutController(IAccountMemoryDB accountMemoryDB)
    {
        _accountMemoryDB = accountMemoryDB;
    }

    [Route("/Logout")]
    [HttpPost]
    public async Task<LogoutResponse> Logout()
    {
        string userName = HttpContext.Request.Headers["User-Name"];

        if (!await _accountMemoryDB.RemoveUser(userName))
        {
            return new LogoutResponse
            {
                Error = ErrorState.FailedConnectRedis
            };
        }

        return new LogoutResponse
        {
            Error = ErrorState.None
        };
    }
}
