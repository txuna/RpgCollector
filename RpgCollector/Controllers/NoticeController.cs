using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RpgCollector.Models;
using RpgCollector.ResponseModels;
using RpgCollector.Services;
using StackExchange.Redis;
using System.Text.Json;

namespace RpgCollector.Controllers
{
    public class NoticeController : Controller
    {
        private ConnectionMultiplexer? redisClient;

        public NoticeController(IOptions<DbConfig> dbConfig)
        {
            redisClient = DatabaseConnector.OpenRedis(dbConfig.Value.RedisDb);
        }

        /*
         * Redis에 저장된 공지사항을 리스트형식으로 뿌려준다. 
         */
        [Route("/Game/Notice")]
        [HttpGet]
        public async Task<JsonResult> Notice()
        {
            NoticeResponse result = await GetAllNotice(); 
            if(redisClient != null) 
            {
                redisClient.CloseRedis();
            }
            if(result == null)
            {
                return Json(new FailResponse
                {
                    Success = false,
                    Message = "Failed Fetch Notice"
                });
            }
            return Json(result);
        }

        public async Task<NoticeResponse?> GetAllNotice()
        {
            if (redisClient == null)
            {
                return null;
            }
            IDatabase redisDB = redisClient.GetDatabase();

            // Redis에 공지사항이 저장되어 있다면
            if (await redisDB.KeyExistsAsync("Notices"))
            {
                RedisValue[] noticesRedis;
                try
                {
                    noticesRedis = await redisDB.ListRangeAsync("Notices");
                }
                catch (Exception ex)
                {
                    return null;
                }
                Notice[] noticesArray = new Notice[noticesRedis.Length];
                for (int i = 0; i < noticesRedis.Length; i++)
                {
                    noticesArray[i] = JsonSerializer.Deserialize<Notice>(noticesRedis[i]);
                }
                NoticeResponse noticeResponse = new NoticeResponse
                {
                    Success = true,
                    NoticeList = noticesArray
                };
                return noticeResponse;
            }
            return null;
        }
    }
}
