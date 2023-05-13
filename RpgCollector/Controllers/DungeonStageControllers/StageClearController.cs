using Microsoft.AspNetCore.Mvc;
using RpgCollector.RequestResponseModel.DungeonStageReqRes;
using RpgCollector.Services;

namespace RpgCollector.Controllers.DungeonStageControllers;

[ApiController]
public class StageClearController : Controller
{
    IDungeonStageDB _dungeonStageDB;
    IRedisMemoryDB _redisMemoryDB;
    ILogger<StageClearController> _logger;
    public StageClearController(IDungeonStageDB dungeonStageDB, IRedisMemoryDB redisMemoryDB, ILogger<StageClearController> logger)
    {
        _dungeonStageDB = dungeonStageDB;
        _redisMemoryDB = redisMemoryDB;
        _logger = logger;
    }

    [Route("/Stage/Clear")]
    [HttpPost]
    public async Task<StageClearResponse> Index(StageClearRequest stageClearRequest)
    {
        // 클리어 검증 

        // 유저 상태 변경 

        // 던전 내용 백업 및 삭제 

        // 경험치 보상 및 파밍 아이템 제공

        // 현재 스테이지 ID와 디베이 저장된 스테이지 ID와 비교하여 같으면 +1 

        return new StageClearResponse
        {
            Error = RequestResponseModel.ErrorCode.None
        };
    }
}
