using Microsoft.AspNetCore.Mvc;
using RpgCollector.Models.MasterModel;
using RpgCollector.Models.PlayerModel;
using RpgCollector.RequestResponseModel;
using RpgCollector.RequestResponseModel.PlayerStateGetModel;
using RpgCollector.Services;
using ZLogger;

namespace RpgCollector.Controllers.PlayerDataController;

[ApiController]
public class PlayerStateGetController : Controller
{
    ILogger<PlayerStateGetController> _logger;
    IPlayerAccessDB _playerAccessDB;
    IMasterDataDB _masterDataDB;
    public PlayerStateGetController(ILogger<PlayerStateGetController> logger, IPlayerAccessDB playerAccessDB, IMasterDataDB masterDataDB)
    {
        _logger = logger;
        _playerAccessDB = playerAccessDB;
        _masterDataDB = masterDataDB;
    }

    [Route("/Player/State")]
    [HttpPost]
    public async Task<PlayerStateGetResponse> PlayerStateGet(PlayerStateGetRequest playerStateGetRequest)
    {
        int userId = Convert.ToInt32(HttpContext.Items["User-Id"]);

        PlayerState? playerState = await _playerAccessDB.GetPlayerFromUserId(userId);

        if(playerState == null)
        {
            return new PlayerStateGetResponse
            {
                Error = ErrorCode.NoneExistName
            };
        }

        MasterPlayerState masterPlayerState = _masterDataDB.GetMasterPlayerState(playerState.Level);

        return new PlayerStateGetResponse
        {
            Error = ErrorCode.None,
            State = playerState,
            MasterState = masterPlayerState
        };
    }
}
