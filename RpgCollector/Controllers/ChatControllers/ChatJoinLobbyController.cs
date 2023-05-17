using Microsoft.AspNetCore.Mvc;
using RpgCollector.RequestResponseModel.ChatReqRes;
using RpgCollector.Services;

namespace RpgCollector.Controllers.ChatControllers
{
    [ApiController]
    public class ChatJoinLobbyController : Controller
    {
        ILogger<ChatJoinLobbyController> _logger;
        IRedisMemoryDB _redisMemoryDB;
        public ChatJoinLobbyController(ILogger<ChatJoinLobbyController> logger, IRedisMemoryDB redisMemoryDB)
        {
            _logger = logger;
            _redisMemoryDB = redisMemoryDB;
        }


        [Route("/Chat/Join")]
        [HttpPost]
        public async Task<ChatJoinLobbyResponse> Post(ChatJoinLobbyRequest chatJoinLobbyRequest)
        {
            return new ChatJoinLobbyResponse
            {
                Error = RequestResponseModel.ErrorCode.None
            };
        }
    }
}
