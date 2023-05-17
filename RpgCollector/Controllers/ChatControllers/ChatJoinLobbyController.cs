using Microsoft.AspNetCore.Mvc;
using RpgCollector.Models.AccountModel;
using RpgCollector.Models.ChatModel;
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

        /**
         * 요청한 UserId가 이미 로비에 있는지 있다면 제거 후 넣기 
         * 1번 로비부터 50개만 
         * 
         */
        [Route("/Chat/Join")]
        [HttpPost]
        public async Task<ChatJoinLobbyResponse> Post(ChatJoinLobbyRequest chatJoinLobbyRequest)
        {
            RedisUser redisUser = (RedisUser)HttpContext.Items["Redis-User"];
            string userName = (string)HttpContext.Items["User-Name"];

            if(await JoinUserInLobby(redisUser, userName) == false)
            {
                return new ChatJoinLobbyResponse
                {
                    Error = RequestResponseModel.ErrorCode.FailedJoinRoom
                };
            }

            return new ChatJoinLobbyResponse
            {
                Error = RequestResponseModel.ErrorCode.None
            };
        }

        async Task<bool> JoinUserInLobby(RedisUser redisUser, string userName)
        {

        }
    }
}
