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

            ChatUser[]? chatUsers = await LoadUserInLobby();
            if(chatUsers == null)
            {
                return new ChatJoinLobbyResponse
                {
                    Error = RequestResponseModel.ErrorCode.FailedLoadLobbyUser
                };
            }

            if(await JoinLobby(chatUsers, redisUser, userName) == false)
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

        async Task<ChatUser[]?> LoadUserInLobby()
        {
            ChatUser[]? chatUsers = await _redisMemoryDB.GetLobbyUser();
            return chatUsers; 
        }

        int FindAvailableLobbyId(ChatUser[] chatUsers)
        {
            for(int i = 1; i<=100; i++)
            {
                int count = chatUsers.Count(user => user.LobbyId == i); 
                if(count < 50)
                {
                    return i;
                }
            }

            // 모두가 50% 이상이라면
            return -1;
        }

        // 이미 있는 유저라면 패스
        async Task<bool> JoinLobby(ChatUser[] chatUsers, RedisUser redisUser, string userName)
        {
            if (AlreadyInLobby(chatUsers, redisUser) == true)
            {
                return false;
            }

            int lobbyId = FindAvailableLobbyId(chatUsers);
            if(lobbyId == -1)
            {
                return false;
            }

            return await _redisMemoryDB.InsertUserInLobby(new ChatUser
            {
                UserId = redisUser.UserId,
                UserName = userName,
                LobbyId = lobbyId
            });
        }

        bool AlreadyInLobby(ChatUser[] chatUsers, RedisUser redisUser)
        {
            return chatUsers.Any(user => user.UserId == redisUser.UserId);
        }
    }
}
