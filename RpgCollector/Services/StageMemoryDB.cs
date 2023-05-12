using CloudStructures;
using Microsoft.Extensions.Options;
using RpgCollector.Models;

namespace RpgCollector.Services;

public interface IStageMemoryDB
{
}
public class StageMemoryDB : IStageMemoryDB
{
    RedisConnection _redisConn;
    ILogger<StageMemoryDB> _logger;
    public StageMemoryDB(IOptions<DbConfig> dbConfig, ILogger<StageMemoryDB> logger)
    {
        var config = new RedisConfig("default", dbConfig.Value.RedisDb);
        _redisConn = new RedisConnection(config);
        _logger = logger;
    }
}
