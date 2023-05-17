using Microsoft.AspNetCore.Mvc;
using RpgCollector.RequestResponseModel.ChatReqRes;
using RpgCollector.Services;

namespace RpgCollector.Controllers.ChatControllers
{
    [ApiController]
    public class ChatExitController : Controller
    {
        ILogger<ChatExitController> _logger;
        IRedisMemoryDB _redisMemoryDB;
        public ChatExitController(ILogger<ChatExitController> logger, IRedisMemoryDB redisMemoryDB)
        {
            _logger = logger;
            _redisMemoryDB = redisMemoryDB;
        }

        [Route("/Chat/Exit")]
        [HttpPost]
        public async Task<ChatExitResponse> Post(ChatExitRequest chatExitRequest)
        {
            return new ChatExitResponse
            {
                Error = RequestResponseModel.ErrorCode.None
            };
        }
    }
}
