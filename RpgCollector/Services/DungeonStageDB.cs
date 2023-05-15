using Microsoft.Extensions.Options;
using MySqlConnector;
using RpgCollector.Models;
using RpgCollector.Models.StageModel;
using RpgCollector.RequestResponseModel;
using SqlKata.Compilers;
using SqlKata.Execution;
using System.Data;
using ZLogger;

namespace RpgCollector.Services;

public interface IDungeonStageDB
{ 
    Task<PlayerStageInfo?> LoadPlayerStageInfo(int userId);
    Task<(ErrorCode, bool)> SetNextStage(int userId, int stageId);
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

    public async Task<(ErrorCode, bool)> SetNextStage(int userId, int stageId)
    {
        try
        {
            int effectedRow = await queryFactory.Query("player_stage_info")
                                                .Where("userId", userId)
                                                .Where("curStageId", stageId)
                                                .UpdateAsync(new
                                                {
                                                    curStageId = stageId + 1
                                                });

            if(effectedRow == 0)
            {
                return (ErrorCode.None, true);
            }
            return (ErrorCode.None, true);
        }
        catch(Exception ex)
        {
            _logger.ZLogError(ex.Message);
            return (ErrorCode.FailedSetNextStage, false);
        }
    }

    public async Task<PlayerStageInfo?> LoadPlayerStageInfo(int userId)
    {
        try
        {
            //TODO:최흥배. 데이터가 아마 없으면 예외가 발생하는 것으로 알고 있습니다.
            // 예외를 발생하지 않는 FirstOrDefaultAsync()를 사용해주세요.
            PlayerStageInfo? info = await queryFactory.Query("player_stage_info")
                                                     .Where("userId", userId)
                                                     .FirstOrDefaultAsync<PlayerStageInfo>();

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
