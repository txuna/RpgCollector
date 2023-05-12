using Microsoft.AspNetCore.Mvc;
using RpgCollector.Models.MasterModel;
using RpgCollector.RequestResponseModel.StageChoiceModel;
using RpgCollector.Services;

namespace RpgCollector.Controllers.DungeonStageControllers;

[ApiController]
public class StageChoiceController : Controller
{
    ILogger<StageChoiceController> _logger;
    IDungeonStageDB _dungeonStageDB;
    IMasterDataDB _masterDataDB;
    public StageChoiceController(IMasterDataDB masterDataDB, IDungeonStageDB dungeonStageDB, ILogger<StageChoiceController> logger)
    {
        _masterDataDB = masterDataDB;
        _dungeonStageDB = dungeonStageDB;
        _logger = logger;
    }

    [Route("/Stage/Choice")]
    [HttpPost]
    public async Task<StageChoiceResponse> Index(StageChoiceRequest stageChoiceRequest)
    {
        int userId = Convert.ToInt32(HttpContext.Items["User-Id"]);

        return new StageChoiceResponse
        {
            Error = RequestResponseModel.ErrorCode.None
        };
    }
}
