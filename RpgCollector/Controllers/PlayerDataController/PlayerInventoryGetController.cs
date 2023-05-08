using Microsoft.AspNetCore.Mvc;
using RpgCollector.Models.PlayerModel;
using RpgCollector.RequestResponseModel.PlayerInventoryGetModel;
using RpgCollector.Services;
using ZLogger;

namespace RpgCollector.Controllers.PlayerDataController
{
    [ApiController]
    public class PlayerInventoryGetController : Controller
    {
        IPlayerAccessDB _playerAccessDB;
        IAccountMemoryDB _accountMemoryDB;
        ILogger<PlayerInventoryGetController> _logger;
        public PlayerInventoryGetController(ILogger<PlayerInventoryGetController> logger, IPlayerAccessDB playerAccessDB, IAccountMemoryDB accountMemoryDB)
        {
            _logger = logger;
            _playerAccessDB = playerAccessDB;
            _accountMemoryDB = accountMemoryDB;
        }

        [Route("/Inventory")]
        [HttpPost]
        public async Task<PlayerInventoryGetResponse> PlayerInventoryGet(PlayerInventoryGetRequest playerInventoryGetRequest)
        {
            string userName = HttpContext.Request.Headers["User-Name"];
            int userId = await _accountMemoryDB.GetUserId(userName);

            _logger.ZLogInformation($"[{userId}] Request /Inventory");

            if (userId == -1)
            {
                return new PlayerInventoryGetResponse
                {
                    Error = RequestResponseModel.ErrorState.NoneExistName
                };
            }

            PlayerItem[] items = await _playerAccessDB.GetPlayerAllItems(userId);

            return new PlayerInventoryGetResponse
            {
                Error = RequestResponseModel.ErrorState.None,
                Items = items
            };
        }
    }
}
