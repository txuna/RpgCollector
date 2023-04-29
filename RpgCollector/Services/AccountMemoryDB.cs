using Microsoft.Extensions.Options;
using RpgCollector.Models;
using RpgCollector.Utility;
using StackExchange.Redis;
using System.Text.Json;

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

    public AccountMemoryDB(IOptions<DbConfig> dbConfig) 
    {
        _dbConfig = dbConfig;
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
        try
        {
            redisClient.Dispose();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
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
            Console.WriteLine(ex.Message);
        }
    }
}
