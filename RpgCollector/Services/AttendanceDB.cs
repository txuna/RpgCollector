using Microsoft.Extensions.Options;
using MySqlConnector;
using RpgCollector.Models;
using RpgCollector.Models.AttendanceData;
using SqlKata.Compilers;
using SqlKata.Execution;
using System.Data;
using System.Data.Common;
using ZLogger;

namespace RpgCollector.Services;

public interface IAttendanceDB
{
    Task<bool> IsAttendance(int userId, string day);
    Task<bool> DoAttendance(int userId, int sequenceDayCount);
    Task<int> GetUserSequenceDayCount(int userId);
    Task<MasterAttendanceReward?> GetAttendanceReward(int day);
    Task<bool> UndoAttendance(int userId, string day);
    Task<PlayerAttendanceInfo?> GetUserAttendanceInfo(int userId);
}

public class AttendanceDB : IAttendanceDB
{
    IDbConnection? dbConnection;
    MySqlCompiler compiler;
    QueryFactory queryFactory;
    IOptions<DbConfig> _dbConfig;
    ILogger<AttendanceDB> _logger;

    public AttendanceDB(IOptions<DbConfig> dbConfig, ILogger<AttendanceDB> logger)
    {
        _dbConfig = dbConfig;
        _logger = logger;
        Open();
    }

    public async Task<bool> UndoAttendance(int userId, string day)
    {
        try
        {
            await queryFactory.Query("player_attendance_info").Where("userId", userId).Where("date", day).DeleteAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.ZLogError(ex.Message);
            return false;
        }
    }

    public async Task<MasterAttendanceReward?> GetAttendanceReward(int day)
    {
        try
        {
            MasterAttendanceReward reward = await queryFactory.Query("master_attendance_reward")
                                                              .Where("dayId", day)
                                                              .FirstAsync<MasterAttendanceReward>();
            return reward;
        }
        catch (Exception ex)
        {
            _logger.ZLogError(ex.Message);
            return null;
        }
    }

    public async Task<PlayerAttendanceInfo?> GetUserAttendanceInfo(int userId)
    {
        try
        {
            PlayerAttendanceInfo info = await queryFactory.Query("player_attendance_info")
                                                          .Where("userId", userId)
                                                          .FirstAsync<PlayerAttendanceInfo>();

            return info;
        }
        catch (Exception ex)
        {
            _logger.ZLogError(ex.Message);
            return null;
        }
    }

    public async Task<int> GetUserSequenceDayCount(int userId)
    {
        try
        {
            int sequenceDayCount = await queryFactory.Query("player_attendance_info")
                                                                        .Where("userId", userId)
                                                                        .Select("sequenceDayCount")
                                                                        .FirstAsync<int>();
            return sequenceDayCount;
        }
        catch (Exception ex)
        {
            _logger.ZLogError(ex.Message);
            return 0;
        }
    }

    public async Task<bool> ExistUser(int userId)
    {
        try
        {
            PlayerAttendanceInfo info = await queryFactory.Query("player_attendance_info")
                                                          .Where("userId", userId)
                                                          .FirstAsync<PlayerAttendanceInfo>();

           if(info == null)
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

    public async Task<bool> DoAttendance(int userId, int sequenceDayCount)
    {
        try
        {
            int effectedRow;
            if (await ExistUser(userId))
            {
                effectedRow = await queryFactory.Query("player_attendance_info")
                                                .Where("userId", userId)
                                                .UpdateAsync(new
                                                {
                                                    sequenceDayCount = sequenceDayCount,
                                                    date = DateTime.Now,
                                                });
            }
            else
            {
                effectedRow = await queryFactory.Query("player_attendance_info")
                                                .Where("userId", userId)
                                                .InsertAsync(new
                                                {
                                                    userId = userId,
                                                    sequenceDayCount = 1,
                                                });
            }
            
            if(effectedRow == 0)
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

    public async Task<bool> IsAttendance(int userId, string day)
    {
        try
        {
            int count = await queryFactory.Query("player_attendance_info")
                                          .Where("date", day)
                                          .Where("userId", userId)
                                          .CountAsync<int>();
            if(count > 0)
            {
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            _logger.ZLogError(ex.Message);
            return false;
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
