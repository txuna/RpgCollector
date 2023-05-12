using Microsoft.AspNetCore.Mvc;
using RpgCollector.Models.AccountModel;
using RpgCollector.Models.MasterModel;
using RpgCollector.Models.StageModel;
using RpgCollector.RequestResponseModel;
using RpgCollector.RequestResponseModel.StageChoiceModel;
using RpgCollector.Services;

namespace RpgCollector.Controllers.DungeonStageControllers;

[ApiController]
public class StageChoiceController : Controller
{
    ILogger<StageChoiceController> _logger;
    IDungeonStageDB _dungeonStageDB;
    IMasterDataDB _masterDataDB;
    IStageMemoryDB _stageMemoryDB;
    IAccountMemoryDB _accountMemoryDB;
    public StageChoiceController(IMasterDataDB masterDataDB, 
                                 IDungeonStageDB dungeonStageDB, 
                                 ILogger<StageChoiceController> logger,
                                 IStageMemoryDB stageMemoryDB,
                                 IAccountMemoryDB accountMemoryDB)
    {
        _masterDataDB = masterDataDB;
        _dungeonStageDB = dungeonStageDB;
        _stageMemoryDB = stageMemoryDB;
        _logger = logger;
        _accountMemoryDB = accountMemoryDB;
    }

    [Route("/Stage/Choice")]
    [HttpPost]
    public async Task<StageChoiceResponse> ChoiceStage(StageChoiceRequest stageChoiceRequest)
    {
        int userId = Convert.ToInt32(HttpContext.Items["User-Id"]);
        string userName = stageChoiceRequest.UserName;

        if(await Verify(stageChoiceRequest.StageId, userId) == false)
        {
            return new StageChoiceResponse
            {
                Error = ErrorCode.NeedClearPreconditionStage
            };
        }

        MasterStageItem[] masterStageItem = LoadStageItem(stageChoiceRequest.StageId);
        MasterStageNpc[] masterStageNpc = LoadStageNpc(stageChoiceRequest.StageId);

        if(await ChangeUserState(userName, UserState.Playing) == false)
        {
            return new StageChoiceResponse
            {
                Error = ErrorCode.RedisErrorCannotEnterStage
            };
        }

        return new StageChoiceResponse
        {
            Error = ErrorCode.None,
            masterStageItem = masterStageItem,
            masterStageNpc = masterStageNpc
        };
    }

    async Task<bool> ChangeUserState(string userName, UserState userState)
    {
        RedisUser? user = await _accountMemoryDB.GetUser(userName);
        if (user == null)
        {
            return false;
        }

        user.State = userState;

        if(await _accountMemoryDB.StoreRedisUser(userName, user) == false)
        {
            return false;
        }

        return true;
    }

    async Task<bool> Verify(int stageId, int userId)
    {
        MasterStageInfo masterStageInfo = _masterDataDB.GetMasterStageInfo(stageId);
        if(masterStageInfo.PreconditionStageId == 0)
        {
            return true;
        }

        PlayerStageInfo? playerStageInfo = await _dungeonStageDB.GetPlayerStageInfo(userId, masterStageInfo.PreconditionStageId);
        if(playerStageInfo == null)
        {
            return false;
        }

        return true;
    }

    MasterStageNpc[] LoadStageNpc(int stageId)
    {
        return _masterDataDB.GetMasterStageNpcs(stageId);
    }

    MasterStageItem[] LoadStageItem(int stageId)
    {
        return _masterDataDB.GetMasterStageItems(stageId);
    }
}
