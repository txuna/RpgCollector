using Microsoft.AspNetCore.Mvc;
using RpgCollector.RequestResponseModel.DungeonStageReqRes;
using RpgCollector.Services;

namespace RpgCollector.Controllers.DungeonStageControllers
{
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
            return new StageClearResponse
            {
                Error = RequestResponseModel.ErrorCode.None
            };
        }
    }
}
