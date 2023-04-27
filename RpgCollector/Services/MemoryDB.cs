using Microsoft.Extensions.Options;
using RpgCollector.Models;
using RpgCollector.ResponseModels;
using StackExchange.Redis;
using System.Text.Json;

namespace RpgCollector.Services
{
    public interface IMemoryDB
    {
        Task<bool> IsDuplicateLogin(string userName);
        Task<bool> StoreUser(User user, string authToken);
        Task<bool> RemoveUser(string userName);
    }

    public class MemoryDB : IMemoryDB
    {
        private ConnectionMultiplexer? redisClient;
        private IDatabase redisDB; 

        public MemoryDB(IOptions<DbConfig> dbConfig) 
        {
            redisClient = DatabaseConnector.OpenRedis(dbConfig.Value.RedisDb);
            if(redisClient != null)
            {
                redisDB = redisClient.GetDatabase();
            }
        }

        public async Task<bool> IsDuplicateLogin(string userName)
        {
            try
            {
                HashEntry[] hashEntries = await redisDB.HashGetAllAsync("Users");
                foreach (HashEntry entry in hashEntries)
                {
                    string? key = entry.Name;
                    if (key == null)
                    {
                        return false;
                    }
                    RedisUser? _redisUser = JsonSerializer.Deserialize<RedisUser>(entry.Value.ToString());
                    if (_redisUser == null)
                    {
                        return false;
                    }
                    if (_redisUser.UserName == userName)
                    {
                        return false;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> StoreUser(User user, string authToken)
        {
            RedisUser redisUser = new RedisUser
            {
                UserId = user.UserId,
                UserName = user.UserName,
                AuthToken = authToken,
                State = UserState.Login,
                TimeStamp = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond
            };
            // 발급한 Token Redis에 저장
            try
            {
                await redisDB.HashSetAsync("Users", user.UserName, JsonSerializer.Serialize(redisUser));
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        public async Task<bool> RemoveUser(string userName)
        {
            try
            {
                await redisDB.HashDeleteAsync("Users", userName);
            }
            catch (Exception ex)
            {
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
