using CloudStructures;
using CloudStructures.Structures;
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
    Task<RedisUser?> GetUser(string userName);
    Task<GameVersion?> GetGameVersion();
}

public class AccountMemoryDB : IAccountMemoryDB
{
    RedisConnection _redisConn;
    ILogger<AccountMemoryDB> _logger;

    public AccountMemoryDB(IOptions<DbConfig> dbConfig, ILogger<AccountMemoryDB> logger) 
    {
        var config = new RedisConfig("default", dbConfig.Value.RedisDb);
        _redisConn = new RedisConnection(config);
        _logger = logger;
    }

    public async Task<GameVersion?> GetGameVersion()
    {
        try
        {
            var redis = new RedisString<GameVersion>(_redisConn, "Version", null);
            var version = await redis.GetAsync();
            if (!version.HasValue)
            {
                return null;
            }
            GameVersion gameVersion = new GameVersion
            {
                ClientVersion = version.Value.ClientVersion,
                MasterVersion = version.Value.MasterVersion
            };
            return gameVersion;
        }
        catch(Exception ex)
        {
            _logger.ZLogError(ex.Message);
            return null;
        }
    }

    public async Task<RedisUser?> GetUser(string userName)
    {
        try
        {
            var redis = new RedisString<RedisUser>(_redisConn , userName, null);
            var user = await redis.GetAsync();
            if (!user.HasValue)
            {
                return null;
            }
            RedisUser redisUser = new RedisUser
            {
                UserId = user.Value.UserId,
                AuthToken = user.Value.AuthToken
            };
            return redisUser;
        }
        catch (Exception ex)
        {
            _logger.ZLogError(ex.Message);
            return null;
        }
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
            var redis = new RedisString<RedisUser>(_redisConn, user.UserName, expiration);
            if(await redis.SetAsync(redisUser, expiration) == false)
            {
                return false;
            }
            return true;
        }
        catch (Exception ex)
        {
            _logger.ZLogError(ex.Message);
            return false;
        }
    }

    public async Task<bool> RemoveUser(string userName)
    {
        try
        {
            var redis = new RedisString<RedisUser>(_redisConn, userName, null);
            var redisResult = await redis.DeleteAsync();
            return redisResult;
        }
        catch (Exception ex)
        {
            _logger.ZLogError(ex.Message);
            return false;
        }
    }
}
