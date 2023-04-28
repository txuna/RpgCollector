using Microsoft.Extensions.Options;
using RpgCollector.Models;
using RpgCollector.RequestResponseModel.NoticeGetModel;
using RpgCollector.RequestResponseModel;
using RpgCollector.Utility;
using StackExchange.Redis;
using System.Text.Json;

namespace RpgCollector.Services
{
    public interface INoticeMemoryDB
    {
        Task<Notice[]?> GetAllNotice();
    }

    public class NoticeMemoryDB : INoticeMemoryDB
    {

        private ConnectionMultiplexer? redisClient;
        private IDatabase redisDB;

        public NoticeMemoryDB(IOptions<DbConfig> dbConfig) 
        {
            redisClient = DatabaseConnector.OpenRedis(dbConfig.Value.RedisDb);

            if (redisClient != null)
            {
                redisDB = redisClient.GetDatabase();
            }
        }

        public async Task<Notice[]?> GetAllNotice()
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
                    Console.WriteLine(ex.Message);
                    return null;
                }

                Notice[] noticesArray = new Notice[noticesRedis.Length];

                for (int i = 0; i < noticesRedis.Length; i++)
                {
                    noticesArray[i] = JsonSerializer.Deserialize<Notice>(noticesRedis[i]);
                }

                NoticeGetResponse noticeResponse = new NoticeGetResponse
                {
                    Error = ErrorState.None,
                    NoticeList = noticesArray
                };

                return noticesArray;
            }
            return null;
        }

        void Dispose()
        {
            if (redisClient != null)
            {
                redisClient.CloseAsync();
            }
        }
    }
}
