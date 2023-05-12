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
        string authToken = Convert.ToString(HttpContext.Items["Auth-Token"]);
        string userName = stageChoiceRequest.UserName;

        if(await Verify(stageChoiceRequest.StageId, userId) == false)
        {
            return new StageChoiceResponse
            {
                Error = ErrorCode.NeedClearPreconditionStage
            };
        }

        StageItem[] stageItem = LoadStageItem(stageChoiceRequest.StageId);
        if(stageItem == null)
        {
            return new StageChoiceResponse
            {
                Error = ErrorCode.FaiedLoadStageItem
            };
        }
        StageNpc[] stageNpc = LoadStageNpc(stageChoiceRequest.StageId);
        if(stageNpc == null)
        {
            return new StageChoiceResponse
            {
                Error = ErrorCode.FailedLoadStageNpc
            };
        }

        if(await ChangeUserState(userName, authToken, userId, UserState.Playing) == false)
        {
            return new StageChoiceResponse
            {
                Error = ErrorCode.RedisErrorCannotEnterStage
            };
        }

        return new StageChoiceResponse
        {
            Error = ErrorCode.None,
            Items = stageItem,
            Npcs = stageNpc
        };
    }

    async Task<bool> ChangeUserState(string userName, string authToken, int userId, UserState userState)
    {
        RedisUser user = new RedisUser
        {
            UserId = userId,
            AuthToken = authToken,
            State = userState
        };

        if(await _accountMemoryDB.StoreRedisUser(userName, user) == false)
        {
            return false;
        }

        return true;
    }

    async Task<bool> Verify(int stageId, int userId)
    {
        PlayerStageInfo? info = await _dungeonStageDB.LoadPlayerStageInfo(userId);
        if (info == null)
        {
            return false;
        }

        if(info.CurStageId < stageId)
        {
            return false;
        }

        return true;
    }

    StageNpc[] LoadStageNpc(int stageId)
    {
        return _masterDataDB.GetMasterStageNpcs(stageId);
    }

    StageItem[] LoadStageItem(int stageId)
    {
        return _masterDataDB.GetMasterStageItems(stageId);
    }
}
