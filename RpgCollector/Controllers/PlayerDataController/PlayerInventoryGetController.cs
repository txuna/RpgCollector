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
        ILogger<PlayerInventoryGetController> _logger;
        public PlayerInventoryGetController(ILogger<PlayerInventoryGetController> logger, 
                                            IPlayerAccessDB playerAccessDB)
        {
            _logger = logger;
            _playerAccessDB = playerAccessDB;
        }

        [Route("/Inventory")]
        [HttpPost]
        public async Task<PlayerInventoryGetResponse> PlayerInventoryGet(PlayerInventoryGetRequest playerInventoryGetRequest)
        {
            int userId = Convert.ToInt32(HttpContext.Items["User-Id"]);

            _logger.ZLogInformation($"[{userId}] Request /Inventory");

            PlayerItem[] items = await _playerAccessDB.GetPlayerAllItems(userId);

            return new PlayerInventoryGetResponse
            {
                Error = RequestResponseModel.ErrorState.None,
                Items = items
            };
        }
    }
}
