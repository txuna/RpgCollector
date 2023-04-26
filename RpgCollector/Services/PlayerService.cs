using Microsoft.Extensions.Options;
using RpgCollector.Models;
using SqlKata.Compilers;
using SqlKata.Execution;
using StackExchange.Redis;
using System.Data;

namespace RpgCollector.Services
{
    public interface IPlayerService
    {
        Task<(bool success, string content)> CreatePlayer(int userId);
    }
    public class PlayerService : IPlayerService
    {
        private IDbConnection? dbConnection;
        private ConnectionMultiplexer? redisClient;

        private MySqlCompiler compiler;
        private QueryFactory queryFactory;

        public PlayerService(IOptions<DbConfig> dbConfig)
        {
            dbConnection = DatabaseSuppoter.OpenMysql(dbConfig.Value.MysqlGameDb); 
            redisClient = DatabaseSuppoter.OpenRedis(dbConfig.Value.RedisDb);
        }
        public async Task<(bool success, string content)> CreatePlayer(int userId)
        {
            Player player = new Player
            {
                UserId = userId,
                CurrentHealth = 0,
                MaxHealth = 0,
                CurrentExp = 0,
                MaxExp = 0,
                Level = 0, 
                Money = 0,

            };
            return (true, "Created");
        }

        public void Dispose()
        {
            if (redisClient != null)
            {
                redisClient.CloseRedis();
            }
            if (dbConnection != null)
            {
                dbConnection.CloseMysql();
            }
        }
    }
}
