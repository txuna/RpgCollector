using Microsoft.AspNetCore.Mvc;
using RpgCollector.Models.AccountModel;
using RpgCollector.Models.ChatModel;
using RpgCollector.RequestResponseModel.ChatReqRes;
using RpgCollector.Services;

namespace RpgCollector.Controllers.ChatControllers
{
    [ApiController]
    public class ChatSendController : Controller
    {
        ILogger<ChatSendController> _logger;
        IRedisMemoryDB _redisMemoryDB;
        public ChatSendController(ILogger<ChatSendController> logger, IRedisMemoryDB redisMemoryDB)
        {
            _logger = logger;
            _redisMemoryDB = redisMemoryDB;
        }

        [Route("/Chat")]
        [HttpPost]
        public async Task<ChatSendResponse> Post(ChatSendRequest chatSendRequest)
        {
            RedisUser user = (RedisUser)HttpContext.Items["Redis-User"];

            int lobbyId = await GetUserLobbyId(user.UserId);
            if(lobbyId == -1)
            {
                return new ChatSendResponse
                {
                    Error = RequestResponseModel.ErrorCode.FailedFindUser
                };
            }

            if(await _redisMemoryDB.UploadChat(lobbyId, new Chat
            {
                UserId = user.UserId,
                UserName = chatSendRequest.UserName,
                Content = chatSendRequest.Content,
                TimeStamp = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond
            }) == false)
            {
                return new ChatSendResponse
                {
                    Error = RequestResponseModel.ErrorCode.FailedSendChat
                };
            }

            return new ChatSendResponse
            {
                Error = RequestResponseModel.ErrorCode.None
            };
        }

        async Task<int> GetUserLobbyId(int userId)
        {
            ChatUser[]? users = await _redisMemoryDB.GetLobbyUser(); 
            if(users == null)
            {
                return -1;
            }

            ChatUser? thisUser = users.Where(user => user.UserId == userId).FirstOrDefault();
            if(thisUser == null)
            {
                return -1;
            }

            return thisUser.LobbyId;
        }
    }
}
