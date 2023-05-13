using CloudStructures;
using CloudStructures.Structures;
using Microsoft.Extensions.Options;
using RpgCollector.Models;
using RpgCollector.Models.AccountModel;
using RpgCollector.Models.StageModel;
using RpgCollector.Utility;
using StackExchange.Redis;
using System.Text.Json;
using ZLogger;

namespace RpgCollector.Services;

public interface IRedisMemoryDB
{
    Task<bool> RemoveUser(string userName);
    Task<bool> StoreUser(string userName, int userId, string authToken, UserState state);
    Task<RedisUser?> GetUser(string userName);
    Task<GameVersion?> GetGameVersion();
    Task<bool> StoreRedisPlayerStageInfo(RedisPlayerStageInfo playerStageInfo, string userName);
}

public class RedisMemoryDB : IRedisMemoryDB
{
    RedisConnection _redisConn;
    ILogger<RedisMemoryDB> _logger;
    readonly string stageKey = "_Stage";

    public RedisMemoryDB(IOptions<DbConfig> dbConfig, ILogger<RedisMemoryDB> logger) 
    {
        var config = new RedisConfig("default", dbConfig.Value.RedisDb);
        _redisConn = new RedisConnection(config);
        _logger = logger;
    }

    public async Task<bool> StoreRedisPlayerStageInfo(RedisPlayerStageInfo playerStageInfo, string userName)
    {
        try
        {
            TimeSpan expiration = TimeSpan.FromMinutes(10);
            var redis = new RedisString<RedisPlayerStageInfo>(_redisConn, userName+stageKey, expiration);
            if(await redis.SetAsync(playerStageInfo, expiration) == false)
            {
                return false;
            }
            return true;
        }
        catch(Exception ex)
        {
            _logger.ZLogError(ex.Message);
            return false;
        }
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
                AuthToken = user.Value.AuthToken,
                State = user.Value.State
            };
            return redisUser;
        }
        catch (Exception ex)
        {
            _logger.ZLogError(ex.Message);
            return null;
        }
    }

    public async Task<bool> StoreUser(string userName, int userId, string authToken, UserState state)
    {
        try
        {
            RedisUser redisUser = new RedisUser
            {
                UserId = userId,
                AuthToken = authToken,
                State = state
            };

            TimeSpan expiration = TimeSpan.FromMinutes(60);
            var redis = new RedisString<RedisUser>(_redisConn, userName, expiration);
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
