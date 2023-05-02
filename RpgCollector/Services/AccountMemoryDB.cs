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
    Task<bool> StoreUser(User user, string authToken);
    Task<bool> RemoveUser(string userName);
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

    public async Task<bool> StoreUser(User user, string authToken)
    {
        try
        {
            TimeSpan expiration = TimeSpan.FromMinutes(60);
            await redisDB.StringSetAsync(user.UserName, authToken, expiration);
        }
        catch (Exception ex)
        {
            _logger.ZLogError(ex.Message);
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
