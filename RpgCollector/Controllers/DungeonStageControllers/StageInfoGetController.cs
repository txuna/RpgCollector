using Microsoft.AspNetCore.Mvc;
using RpgCollector.RequestResponseModel.StageInfoGetModel;
using RpgCollector.Services;

namespace RpgCollector.Controllers.DungeonStageControllers
{
    [ApiController]
    public class StageInfoGetController : Controller
    {
        ILogger<StageInfoGetController> _logger;
        public StageInfoGetController(ILogger<StageInfoGetController> logger)
        {
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
