using Microsoft.Extensions.Options;
using MySqlConnector;
using RpgCollector.Models;
using RpgCollector.Models.AttendanceData;
using SqlKata.Compilers;
using SqlKata.Execution;
using System.Data;
using System.Data.Common;
using ZLogger;

namespace RpgCollector.Services
{
    public interface IAttendanceDB
    {
        Task<bool> IsAttendance(int userId, string day);
        Task<bool> DoAttendance(int userId, int sequenceDayCount);
        Task<PlayerAttendanceLog?> GetLastAttendanceLog(int userId);
        Task<MasterAttendanceReward?> GetAttendanceReward(int day);
        Task<bool> UndoAttendance(int userId, string day);
        Task<PlayerAttendanceLog?> GetLastSequenceDayCount(int userId);
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

        public async Task<PlayerAttendanceLog?> GetLastSequenceDayCount(int userId)
        {
            try
            {
                PlayerAttendanceLog log = await queryFactory.Query("player_attendance_log")
                                                            .Where("userId", userId)
                                                            .OrderByDesc("date")
                                                            .FirstAsync<PlayerAttendanceLog>();
                return log;
            }
            catch (Exception ex)
            {
                _logger.ZLogError(ex.Message);
                return null;
            }
        }

        public async Task<bool> UndoAttendance(int userId, string day)
        {
            try
            {
                await queryFactory.Query("player_attendance_log").Where("userId", userId).Where("date", day).DeleteAsync();
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

        // 가장 최근 출석날짜와  sequence를 가지고옴
        // 최근 날짜가 
        public async Task<PlayerAttendanceLog?> GetLastAttendanceLog(int userId)
        {
            try
            {
                PlayerAttendanceLog playerAttendanceLog = await queryFactory.Query("player_attendance_log")
                                                                            .Where("userId", userId)
                                                                            .OrderByDesc("date")
                                                                            .FirstAsync<PlayerAttendanceLog>();
                return playerAttendanceLog;
            }
            catch (Exception ex)
            {
                _logger.ZLogError(ex.Message);
                return null;
            }
        }

        public async Task<bool> DoAttendance(int userId, int sequenceDayCount)
        {
            try
            {
                await queryFactory.Query("player_attendance_log").InsertAsync(new
                {
                    userId = userId,
                    sequenceDayCount = sequenceDayCount
                });
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
                int count = await queryFactory.Query("player_attendance_log").Where("date", day).Where("userId", userId).CountAsync<int>();
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
}
