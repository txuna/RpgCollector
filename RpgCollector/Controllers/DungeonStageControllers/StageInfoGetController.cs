using Microsoft.AspNetCore.Mvc;
using RpgCollector.Models.MasterModel;
using RpgCollector.Models.StageModel;
using RpgCollector.RequestResponseModel;
using RpgCollector.RequestResponseModel.StageInfoGetModel;
using RpgCollector.Services;
using ZLogger;

namespace RpgCollector.Controllers.DungeonStageControllers;

[ApiController]
public class StageInfoGetController : Controller
{
    ILogger<StageInfoGetController> _logger;
    IDungeonStageDB _dungeonStageDB;
    IMasterDataDB _masterDataDB;
    public StageInfoGetController(ILogger<StageInfoGetController> logger, IDungeonStageDB dungeonStageDB, IMasterDataDB masterDataDB)
    {
        _logger = logger;
        _dungeonStageDB = dungeonStageDB;
        _masterDataDB = masterDataDB;
    }

    /**
     * 각 스테이지 별로  OPEN / CLOSE 값을 줌
     */
    [Route("/Stage/Info")]
    [HttpPost]
    public async Task<StageInfoGetResponse> StageInfoGet(StageInfoGetRequest stageInfoGetRequest)
    {
        int userId = Convert.ToInt32(HttpContext.Items["User-Id"]);
        Stage[] stage = await ProcessingStage(userId);

        if(stage == null)
        {
            return new StageInfoGetResponse
            {
                Error = ErrorCode.FailedFetchStageInfo
            };

        }

        return new StageInfoGetResponse
        {
            Error = ErrorCode.None,
            Stages = stage
        };
    }

    async Task<Stage[]?> ProcessingStage(int userId)
    {
        MasterStageInfo[] masterStageInfo = _masterDataDB.GetMasterStageInfoList();
        PlayerStageInfo[]? playerStageInfo = await _dungeonStageDB.GetPlyerStageInfo(userId);

        if(playerStageInfo == null)
        {
            return null;
        }

        Stage[] stage = new Stage[masterStageInfo.Length];
        for (int i = 0; i < masterStageInfo.Length; i++)
        {
            stage[i] = new Stage
            {
                StageId = masterStageInfo[i].StageId
            };
            if (masterStageInfo[i].PreconditionStageId == 0)
            {
                stage[i].IsOpen = true;
            }
            else
            {
                try
                {
                    PlayerStageInfo info = playerStageInfo.First(e => e.StageId == masterStageInfo[i].PreconditionStageId);
                    if(info == null)
                    {
                        stage[i].IsOpen = false;
                    }
                    else
                    {
                        stage[i].IsOpen = true;
                    }
                }
                catch(Exception ex)
                {
                    _logger.ZLogError(ex.Message);
                    stage[i].IsOpen = false;
                }
            }
        }

        return stage;
    }
}
