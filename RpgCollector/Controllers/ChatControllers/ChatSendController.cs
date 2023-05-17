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
            bool result = await _redisMemoryDB.UploadChat(new Chat
            {
                UserName = chatSendRequest.UserName,
                Content = chatSendRequest.Content,
                Date = DateTime.Now.ToString("yyyy-MM-dd")

        });

            if(result == false)
            {
                Console.WriteLine("FALSE");
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
    }
}
