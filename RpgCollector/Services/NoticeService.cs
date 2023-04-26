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
        private IDbConnection? _dbConnection;
        private ConnectionMultiplexer? redisClient;

        private MySqlCompiler _compiler;
        private QueryFactory _queryFactory;


        public NoticeService(IOptions<DbConfig> dbConfig)
        {
            _dbConnection = DatabaseSuppoter.OpenMysql(dbConfig.Value.MysqlGameDb);
            redisClient = DatabaseSuppoter.OpenRedis(dbConfig.Value.RedisDb);

            if (IsOpenDB())
            {
                _compiler = new MySqlCompiler();
                _queryFactory = new QueryFactory(_dbConnection, _compiler);
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
                RedisValue[] noticesRedis = await redisDB.ListRangeAsync("Notices");
                NoticeResponse[] noticesArray = new NoticeResponse[noticesRedis.Length];
                for (int i = 0; i < noticesRedis.Length; i++)
                {
                    noticesArray[i] = JsonSerializer.Deserialize<NoticeResponse>(noticesRedis[i]);
                }
                return (true, JsonSerializer.Serialize(noticesArray));
            }
            // 데이터베이스에 존재하는 공지사항을 가지고와서 Redis에 저장한다. 
            IEnumerable<Notice> noticesData = await _queryFactory.Query("notices").GetAsync<Notice>();
            foreach (Notice value in noticesData)
            {
                await redisDB.ListRightPushAsync("Notices", JsonSerializer.Serialize(value));
            }
            return (true, JsonSerializer.Serialize(noticesData));
        }

        public bool IsOpenDB()
        {
            if (redisClient != null && _dbConnection != null)
            {
                return true;
            }
            return false;
        }

    }
}
