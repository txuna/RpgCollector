using Microsoft.AspNetCore.Mvc;
using RpgCollector.Models.MasterModel;
using RpgCollector.Models.PlayerModel;
using RpgCollector.RequestResponseModel;
using RpgCollector.RequestResponseModel.PlayerStateGetModel;
using RpgCollector.Services;
using ZLogger;

namespace RpgCollector.Controllers.PlayerDataController
{
    [ApiController]
    public class PlayerStateGetController : Controller
    {
        IAccountMemoryDB _accountMemoryDB;
        ILogger<PlayerStateGetController> _logger;
        IPlayerAccessDB _playerAccessDB;
        IMasterDataDB _masterDataDB;
        public PlayerStateGetController(ILogger<PlayerStateGetController> logger, IPlayerAccessDB playerAccessDB, IAccountMemoryDB accountMemoryDB, IMasterDataDB masterDataDB)
        {
            _logger = logger;
            _playerAccessDB = playerAccessDB;
            _accountMemoryDB = accountMemoryDB;
            _masterDataDB = masterDataDB;
        }
        /**
         * 요청 행위자(userId)와 요청 userId의 일치성 확인 필요
         */
        [Route("/Player/State")]
        [HttpPost]
        public async Task<PlayerStateGetResponse> PlayerStateGet(PlayerStateGetRequest playerStateGetRequest)
        {
            string userName = HttpContext.Request.Headers["User-Name"];
            int userId = await _accountMemoryDB.GetUserId(userName);

            _logger.ZLogInformation($"[{userId}] Request /Player/State");

            if(userId == -1)
            {
                return new PlayerStateGetResponse
                {
                    Error = ErrorState.NoneExistName
                };
            }

            PlayerState? playerState = await _playerAccessDB.GetPlayerFromUserId(userId);

            if(playerState == null)
            {
                return new PlayerStateGetResponse
                {
                    Error = ErrorState.NoneExistName
                };
            }

            MasterPlayerState masterPlayerState = _masterDataDB.GetMasterPlayerState(playerState.Level);

            return new PlayerStateGetResponse
            {
                Error = ErrorState.None,
                State = playerState,
                MasterState = masterPlayerState
            };
        }
    }
}
