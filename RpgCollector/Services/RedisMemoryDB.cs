using CloudStructures;
using CloudStructures.Structures;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RpgCollector.Models;
using RpgCollector.Models.AccountModel;
using RpgCollector.Models.ChatModel;
using RpgCollector.Models.StageModel;
using RpgCollector.Utility;
using StackExchange.Redis;
using System.Text.Json;
using ZLogger;

namespace RpgCollector.Services;

public interface IRedisMemoryDB
{
    Task<bool> RemoveUser(string userName);
    Task<bool> StoreRedisUser(string userName, RedisUser user);
    Task<bool> StoreUser(string userName, int userId, string authToken, UserState state);
    Task<RedisUser?> GetUser(string userName);
    Task<GameVersion?> GetGameVersion();
    Task<bool> StoreRedisPlayerStageInfo(RedisPlayerStageInfo playerStageInfo, string userName);
    Task<bool> RemoveRedisPlayerStageInfo(string userName);
    Task<RedisPlayerStageInfo?> GetRedisPlayerStageInfo(string userName);
    Task<bool> UploadChat(int lobbyId, Chat chat);
    Task<bool> InsertUserInLobby(ChatUser chatUser);
    Task<ChatUser[]?> GetLobbyUser();
    Task<Chat[]?> LoadChat(int lobbyId);
}

public class RedisMemoryDB : IRedisMemoryDB
{
    RedisConnection _redisConn;
    ILogger<RedisMemoryDB> _logger;
    string redisStageKey = string.Empty;
    readonly int loginExpireTime = 60;
    readonly int stageExpireTime = 10;
    IOptions<ConfigLobby> _configLobby; 

    public RedisMemoryDB(IOptions<DbConfig> dbConfig, ILogger<RedisMemoryDB> logger, IOptions<ConfigLobby> configLobby) 
    {
        //ConfigurationOptions option = ConfigurationOptions.Parse(dbConfig.Value.RedisDb);
        //option.Password = dbConfig.Value.RedisPassword;
        var config = new RedisConfig("default", dbConfig.Value.RedisDb);
        //var config = new RedisConfig("default", option);
        redisStageKey = dbConfig.Value.RedisStageSecretKey;
        _redisConn = new RedisConnection(config);
        _logger = logger;
        _configLobby = configLobby;
    }

    public async Task<RedisPlayerStageInfo?> GetRedisPlayerStageInfo(string userName)
    {
        try
        {
            var redis = new RedisString<RedisPlayerStageInfo>(_redisConn, userName+ redisStageKey, null);
            var info = await redis.GetAsync();
            if (!info.HasValue)
            {
                return null;
            }

            RedisPlayerStageInfo playerStageInfo = new RedisPlayerStageInfo
            {
                UserId = info.Value.UserId,
                StageId = info.Value.StageId,
                FarmingItems = info.Value.FarmingItems,
                Npcs = info.Value.Npcs,
                RewardExp = info.Value.RewardExp,
            };

            return playerStageInfo;
        }
        catch (Exception ex)
        {
            _logger.ZLogError(ex.Message);
            return null;
        }
    }

    public async Task<bool> RemoveRedisPlayerStageInfo(string userName)
    {
        try
        {
            var redis = new RedisString<RedisPlayerStageInfo>(_redisConn, userName+ redisStageKey, null); // 키 확인
            var redisResult = await redis.DeleteAsync();
            if(redisResult == false)
            {
                return true;
            }
            return redisResult;
        }
        catch (Exception ex)
        {
            _logger.ZLogError(ex.Message);
            return false;
        }
    }

    public async Task<bool> StoreRedisPlayerStageInfo(RedisPlayerStageInfo playerStageInfo, string userName)
    {
        try
        {
            TimeSpan expiration = GetStageUserExpireTime();
            var redis = new RedisString<RedisPlayerStageInfo>(_redisConn, userName+redisStageKey, expiration);
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

    public async Task<bool> StoreRedisUser(string userName, RedisUser user)
    {
        try
        {
            TimeSpan expiration = GetLoginUserExpireTime();
            var redis = new RedisString<RedisUser>(_redisConn, userName, expiration);
            if (await redis.SetAsync(user, expiration) == false)
            {
                return false;
            }
            return true;
        }
        catch(Exception ex )
        {
            _logger.ZLogError(ex.Message);
            return false;
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

            TimeSpan expiration = GetLoginUserExpireTime();
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

    public async Task<ChatUser[]?> GetLobbyUser()
    {
        try
        {
            var redis = new RedisSet<ChatUser>(_redisConn, "Lobby", null);
            ChatUser[] users = await redis.MembersAsync();
            return users;
        }
        catch(Exception ex)
        {
            _logger.ZLogError(ex.Message);
            return null;
        }
    }

    public async Task<bool> InsertUserInLobby(ChatUser chatUser)
    {
        try
        {
            var redis = new RedisSet<ChatUser>(_redisConn, "Lobby", null);
            return await redis.AddAsync(chatUser);
        }
        catch(Exception ex)
        {
            _logger.ZLogError(ex.Message);
            return false;
        }
    }

    public async Task<Chat[]?> LoadChat(int lobbyId)
    {
        try
        {
            string lobbyName = Convert.ToString(lobbyId) + _configLobby.Value.LobbySuffix;
            var redis = new RedisList<Chat>(_redisConn, lobbyName, null);
            return await redis.RangeAsync(0, -1);
        }
        catch(Exception ex)
        {
            _logger.ZLogError(ex.Message);
            return null;
        }
    }

    public async Task<bool> UploadChat(int lobbyId, Chat chat)
    {
        try
        {
            var script =
@"local max = tonumber(ARGV[1]) 
local l = tonumber(redis.call('llen', KEYS[1])) 
if(l < max) then 
	redis.call('rpush', KEYS[1], ARGV[2]) 
else 
	redis.call('lpop', KEYS[1]) 
    redis.call('rpush', KEYS[1], ARGV[2]) 
end 
return l
";
            string lobbyName = Convert.ToString(lobbyId) + _configLobby.Value.LobbySuffix;
            var redis = new RedisLua(_redisConn, lobbyName);
            var keys = new RedisKey[] { lobbyName };
            var values = new RedisValue[] {5, JsonConvert.SerializeObject(chat)};
            await redis.ScriptEvaluateAsync(script, keys, values);
            return true;
        }
        catch (Exception ex)
        {
            _logger.ZLogError(ex.Message);
            return false;
        }
    }

    TimeSpan GetLoginUserExpireTime()
    {
        TimeSpan expiration = TimeSpan.FromMinutes(loginExpireTime);
        return expiration;
    }

    TimeSpan GetStageUserExpireTime()
    {
        TimeSpan expiration = TimeSpan.FromMinutes(stageExpireTime);
        return expiration;
    }
}
