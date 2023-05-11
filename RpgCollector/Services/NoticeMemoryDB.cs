using Microsoft.Extensions.Options;
using RpgCollector.Models;
using RpgCollector.RequestResponseModel.NoticeGetModel;
using RpgCollector.RequestResponseModel;
using RpgCollector.Utility;
using StackExchange.Redis;
using System.Text.Json;
using ZLogger;
using RpgCollector.Models.NoticeModel;
using CloudStructures;
using CloudStructures.Structures;

namespace RpgCollector.Services;

public interface INoticeMemoryDB
{
    Task<Notice[]?> GetAllNotice();
}

public class NoticeMemoryDB : INoticeMemoryDB
{
    RedisConnection redisConn;
    ILogger<NoticeMemoryDB> _logger;

    public NoticeMemoryDB(IOptions<DbConfig> dbConfig, ILogger<NoticeMemoryDB> logger) 
    {
        var config = new RedisConfig("default", dbConfig.Value.RedisDb);
        redisConn = new RedisConnection(config);
        _logger = logger;
    }

    public async Task<Notice[]?> GetAllNotice()
    {
        try
        {
            var redis = new RedisList<Notice>(redisConn, "Notice", null);
            Notice[] notices = await redis.RangeAsync(0, -1);
            return notices;
        }
        catch (Exception ex)
        {
            _logger.ZLogError(ex.Message);
            return null;
        }
    }
}
