using Microsoft.Extensions.Options;
using RpgCollector.Models;
using RpgCollector.Utility;
using StackExchange.Redis;
using System.Text.Json;

namespace RpgCollector.Services
{
    public interface IAccountMemoryDB
    {
        Task<bool> StoreUser(User user, string authToken);
        Task<bool> RemoveUser(string userName);
    }

    public class AccountMemoryDB : IAccountMemoryDB
    {
        private ConnectionMultiplexer? redisClient;
        private IDatabase redisDB; 

        public AccountMemoryDB(IOptions<DbConfig> dbConfig) 
        {
            redisClient = DatabaseConnector.OpenRedis(dbConfig.Value.RedisDb);
            if(redisClient != null)
            {
                redisDB = redisClient.GetDatabase();
            }
        }

        public async Task<bool> StoreUser(User user, string authToken)
        {
            try
            {
                TimeSpan expiration = TimeSpan.FromMinutes(60);
                await redisDB.StringSetAsync(user.UserName, authToken, expiration);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
            return true;
        }

        public async Task<bool> RemoveUser(string userName)
        {
            try
            {
                await redisDB.KeyDeleteAsync(userName);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
            return true;
        }

        void Dispose()
        {
            if (redisClient != null)
            {
                redisClient.CloseRedis();
            }
        }
    }
}
