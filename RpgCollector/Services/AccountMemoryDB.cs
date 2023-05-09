using Microsoft.Extensions.Options;
using RpgCollector.Models;
using RpgCollector.Models.AccountModel;
using RpgCollector.Utility;
using StackExchange.Redis;
using System.Text.Json;
using ZLogger;

namespace RpgCollector.Services;

public interface IAccountMemoryDB
{
    Task<bool> RemoveUser(string userName);
    Task<bool> StoreRedisUser(User user, string authToken);
    Task<int> GetUserId(string userName);
}

public class AccountMemoryDB : IAccountMemoryDB
{
    private ConnectionMultiplexer? redisClient;
    private IDatabase redisDB;
    IOptions<DbConfig> _dbConfig;
    ConnectionMultiplexer _redisClient;
    ILogger<AccountMemoryDB> _logger;

    public AccountMemoryDB(IOptions<DbConfig> dbConfig, ILogger<AccountMemoryDB> logger) 
    {
        _dbConfig = dbConfig;
        _logger = logger;
        Open();
    }

    public async Task<bool> StoreRedisUser(User user, string authToken)
    {
        try
        {
            RedisUser redisUser = new RedisUser
            {
                UserId = user.UserId,
                AuthToken = authToken
            };

            TimeSpan expiration = TimeSpan.FromMinutes(60);
            await redisDB.StringSetAsync(user.UserName, JsonSerializer.Serialize(redisUser), expiration);

            return true;
        }
        catch (Exception ex)
        {
            _logger.ZLogError(ex.Message);
            return false;
        }
    }

    public async Task<int> GetUserId(string userName)
    {
        try
        {
            string stringUser = await redisDB.StringGetAsync(userName);
            RedisUser redisUser = JsonSerializer.Deserialize<RedisUser>(stringUser);

            return redisUser.UserId;
        }
        catch (Exception ex)
        {
            _logger.ZLogError(ex.Message);
            return -1;
        }
    }

    public async Task<bool> RemoveUser(string userName)
    {
        try
        {
            await redisDB.KeyDeleteAsync(userName);
        }
        catch (Exception ex)
        {
            _logger.ZLogError(ex.Message);
            return false;
        }
        return true;
    }

    void Dispose()
    {
        try
        {
            redisClient.Dispose();
        }
        catch (Exception ex)
        {
            _logger.ZLogError(ex.Message);
        }
    }

    void Open()
    {
        ConfigurationOptions option = new ConfigurationOptions
        {
            EndPoints = { _dbConfig.Value.RedisDb }
        };
        try
        {
            redisClient = ConnectionMultiplexer.Connect(option);
            redisDB = redisClient.GetDatabase();
        }
        catch (Exception ex)
        {
            _logger.ZLogError(ex.Message);
        }
    }
}
