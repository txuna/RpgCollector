using Microsoft.AspNetCore.Mvc;
using RpgCollector.RequestResponseModel.StageInfoGetModel;
using RpgCollector.Services;

namespace RpgCollector.Controllers.DungeonStageControllers
{
    [ApiController]
    public class StageInfoGetController : Controller
    {
        ILogger<StageInfoGetController> _logger;
        IAccountMemoryDB _accountMemoryDB;
        public StageInfoGetController(ILogger<StageInfoGetController> logger, IAccountMemoryDB accountMemoryDB)
        {
            _accountMemoryDB = accountMemoryDB;
            _logger = logger;
        }

        [Route("/Stage/Info")]
        [HttpPost]
        public async Task<StageInfoGetResponse> StageInfoGet(StageInfoGetRequest stageInfoGetRequest)
        {
            return new StageInfoGetResponse
            {
                Error = RequestResponseModel.ErrorState.None
            };
        }
    }
}
