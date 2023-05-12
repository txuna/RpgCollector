using Microsoft.Extensions.Options;
using MySqlConnector;
using RpgCollector.Models;
using RpgCollector.Models.StageModel;
using SqlKata.Compilers;
using SqlKata.Execution;
using System.Data;
using ZLogger;

namespace RpgCollector.Services;

public interface IDungeonStageDB
{
    Task<PlayerStageInfo[]?> GetAllPlyerStageInfo(int userId);
    Task<PlayerStageInfo?> GetPlayerStageInfo(int userId, int stageId);
}

public class DungeonStageDB : IDungeonStageDB
{
    IDbConnection dbConnection;
    MySqlCompiler compiler;
    QueryFactory queryFactory;
    IOptions<DbConfig> _dbConfig;
    ILogger<DungeonStageDB> _logger; 

    public DungeonStageDB(IOptions<DbConfig> dbConfig, ILogger<DungeonStageDB> logger)
    {
        _dbConfig = dbConfig;
        _logger = logger;
        Open();
    }

    public async Task<PlayerStageInfo?> GetPlayerStageInfo(int userId, int stageId)
    {
        try
        {
            PlayerStageInfo info = await queryFactory.Query("player_stage_clear_info")
                                                     .Where("stageId", stageId)
                                                     .Where("userId", userId)
                                                     .FirstAsync<PlayerStageInfo>();

            return info;
        }
        catch(Exception ex)
        {
            _logger.ZLogError(ex.Message);
            return null;
        }
    }

    public async Task<PlayerStageInfo[]?> GetAllPlyerStageInfo(int userId)
    {
        try
        {
            IEnumerable<PlayerStageInfo> info = await queryFactory.Query("player_stage_clear_info")
                                                                  .Where("userId", userId)
                                                                  .GetAsync<PlayerStageInfo>();

            return info.ToArray();
        }
        catch(Exception ex)
        {
            _logger.ZLogError(ex.Message);
            return null;
        }
    }

    void Dispose()
    {
        try
        {
            dbConnection.Close();
        }
        catch (Exception ex)
        {
            _logger.ZLogError(ex.Message);
        }
    }

    void Open()
    {
        try
        {
            dbConnection = new MySqlConnection(_dbConfig.Value.MysqlGameDb);
            dbConnection.Open();
            compiler = new MySqlCompiler();
            queryFactory = new QueryFactory(dbConnection, compiler);
        }
        catch (Exception ex)
        {
            _logger.ZLogError(ex.Message);
        }
    }
}
