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
    Task<PlayerStageInfo?> LoadPlayerStageInfo(int userId);
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

    public async Task<PlayerStageInfo?> LoadPlayerStageInfo(int userId)
    {
        try
        {
            PlayerStageInfo info = await queryFactory.Query("player_stage_info")
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
