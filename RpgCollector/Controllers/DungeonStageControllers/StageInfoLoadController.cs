using Microsoft.AspNetCore.Mvc;
using RpgCollector.Models.MasterModel;
using RpgCollector.Models.StageModel;
using RpgCollector.RequestResponseModel;
using RpgCollector.RequestResponseModel.DungeonStageReqRes;
using RpgCollector.Services;
using ZLogger;

namespace RpgCollector.Controllers.DungeonStageControllers;

[ApiController]
public class StageInfoLoadController : Controller
{
    ILogger<StageInfoLoadController> _logger;
    IDungeonStageDB _dungeonStageDB;
    IMasterDataDB _masterDataDB;
    public StageInfoLoadController(ILogger<StageInfoLoadController> logger, IDungeonStageDB dungeonStageDB, IMasterDataDB masterDataDB)
    {
        _logger = logger;
        _dungeonStageDB = dungeonStageDB;
        _masterDataDB = masterDataDB;
    }

    /**
     * 플레이어의 현재 stage 단계를 보여줌 
     */
    [Route("/Stage/Info")]
    [HttpPost]
    public async Task<StageInfoGetResponse> StageInfoLoad(StageInfoGetRequest stageInfoGetRequest)
    {
        int userId = Convert.ToInt32(HttpContext.Items["User-Id"]);

        PlayerStageInfo? info = await _dungeonStageDB.LoadPlayerStageInfo(userId);

        if(info == null)
        {
            return new StageInfoGetResponse
            {
                Error = ErrorCode.FailedFetchStageInfo
            };
        }
        
        return new StageInfoGetResponse
        {
            Error = ErrorCode.None,
            CurStageId = info.CurStageId
        };
    }
}
