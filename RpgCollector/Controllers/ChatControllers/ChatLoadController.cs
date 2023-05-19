using Microsoft.AspNetCore.Mvc;
using RpgCollector.Models.AccountModel;
using RpgCollector.Models.ChatModel;
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
            RedisUser redisUser = (RedisUser)HttpContext.Items["Redis-User"];

            int lobbyId = await FindUserLobbyId(redisUser.UserId);
            if(lobbyId == -1)
            {
                return new ChatLoadResponse
                {
                    Error = RequestResponseModel.ErrorCode.FailedFindUser
                };
            }

            Chat[]? chats = await LoadChat(lobbyId, chatLoadResquest.TimeStamp);
            if(chats == null)
            {
                return new ChatLoadResponse
                {
                    Error = RequestResponseModel.ErrorCode.FailedLoadChat
                };
            }

            Int64 lastTimeStamp = GetLastTimeStamp(chats);
            if(lastTimeStamp == 0)
            {
                lastTimeStamp = chatLoadResquest.TimeStamp;
            }

            return new ChatLoadResponse
            {
                Error = RequestResponseModel.ErrorCode.None,
                ChatLog = chats, 
                TimeStamp = lastTimeStamp
            };
        }

        Int64 GetLastTimeStamp(Chat[] chats)
        {
            var chat = chats.OrderByDescending(chat => chat.TimeStamp).FirstOrDefault();
            if(chat == null)
            {
                return 0;
            }

            return chat.TimeStamp;
        }

        // 클라이언트가 보낸 타임스탬프 보다 큰 것만 보낸다.
        async Task<Chat[]?> LoadChat(int lobbyId, Int64 timeStamp)
        {
            Chat[]? chats = await _redisMemoryDB.LoadChat(lobbyId);
            if(chats == null)
            {
                return null;
            }

            chats = chats.Where(chat => chat.TimeStamp > timeStamp).ToArray();
            return chats;
        }

        async Task<int> FindUserLobbyId(int userId)
        {
            ChatUser[]? users = await _redisMemoryDB.GetLobbyUser();
            if(users == null)
            {
                return -1;
            }

            ChatUser? chatUser = users.Where(user => user.UserId == userId).FirstOrDefault();    
            if(chatUser == null)
            {
                return -1;
            }

            return chatUser.LobbyId;
        }
    }
}
