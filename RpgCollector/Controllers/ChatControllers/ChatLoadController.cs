using Microsoft.AspNetCore.Mvc;
using RpgCollector.RequestResponseModel.ChatReqRes;
using RpgCollector.Services;

namespace RpgCollector.Controllers.ChatControllers
{
    [ApiController]
    public class ChatLoadController : Controller
    {
        IRedisMemoryDB _redisMemoryDB;
        ILogger<ChatLoadController> _logger; 
        public ChatLoadController(IRedisMemoryDB redisMemoryDB, ILogger<ChatLoadController> logger)
        {
            _logger = logger;
            _redisMemoryDB = redisMemoryDB;
        }

        [Route("/Chat/Load")]
        [HttpPost]
        public async Task<ChatLoadResponse> Post(ChatLoadResquest chatLoadResquest)
        {
            return new ChatLoadResponse
            {
                Error = RequestResponseModel.ErrorCode.None
            };
        }
    }
}
