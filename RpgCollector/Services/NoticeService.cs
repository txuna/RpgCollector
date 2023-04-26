using Microsoft.Extensions.Options;
using RpgCollector.Models;
using RpgCollector.ResponseModels;
using SqlKata.Compilers;
using SqlKata.Execution;
using StackExchange.Redis;
using System.Data;
using System.Text.Json;

namespace RpgCollector.Services
{
    public interface INoticeService
    {
        Task<(bool success, string content)> GetAllNotice();
    }

    public class NoticeService : INoticeService
    {
        private IDbConnection? dbConnection;
        private ConnectionMultiplexer? redisClient;

        private MySqlCompiler compiler;
        private QueryFactory queryFactory;


        public NoticeService(IOptions<DbConfig> dbConfig)
        {
            dbConnection = DatabaseSuppoter.OpenMysql(dbConfig.Value.MysqlGameDb);
            redisClient = DatabaseSuppoter.OpenRedis(dbConfig.Value.RedisDb);

            if (IsOpenDB())
            {
                compiler = new MySqlCompiler();
                queryFactory = new QueryFactory(dbConnection, compiler);
            }
        }
        /*
         * Redis에서 Notices를 읽어온다. 
         * 만약 Redis의 Notices키가 없거나 내용물이 비어있을 경우 데이터베이스에서 로드 후 Redis에 저장
         * 만약 성공적으로 값이 읽힌다면 리스트를 JSON형식으로 반환한다. 
         */
        public async Task<(bool success, string content)> GetAllNotice()
        {
            if(!IsOpenDB())
            {
                return (false, "Database Connection Failed");
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
                catch(Exception ex)
                {
                    return (false, "Database Connection Failed");
                }
                NoticeResponse[] noticesArray = new NoticeResponse[noticesRedis.Length];
                for (int i = 0; i < noticesRedis.Length; i++)
                {
                    noticesArray[i] = JsonSerializer.Deserialize<NoticeResponse>(noticesRedis[i]);
                }
                return (true, JsonSerializer.Serialize(noticesArray));
            }
            // 데이터베이스에 존재하는 공지사항을 가지고와서 Redis에 저장한다. 
            try
            {
                IEnumerable<Notice> noticesData = await queryFactory.Query("notices").GetAsync<Notice>();
                foreach (Notice value in noticesData)
                {
                    await redisDB.ListRightPushAsync("Notices", JsonSerializer.Serialize(value));
                }
                return (true, JsonSerializer.Serialize(noticesData));
            }
            catch (Exception ex)
            {
                return (false, "Database Connection Failed");
            }
            
        }

        public bool IsOpenDB()
        {
            if (redisClient != null && dbConnection != null)
            {
                return true;
            }
            return false;
        }

    }
}
