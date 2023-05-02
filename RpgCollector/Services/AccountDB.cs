using Microsoft.Extensions.Options;
using MySqlConnector;
using RpgCollector.Models;
using RpgCollector.Models.AccountModel;
using RpgCollector.Utility;
using SqlKata;
using SqlKata.Compilers;
using SqlKata.Execution;
using StackExchange.Redis;
using System.Data;
using System.Text.Json;
using ZLogger;

namespace RpgCollector.Services;

public interface IAccountDB
{
    Task<User?> GetUser(string userName);
    Task<int> RegisterUser(string userName, string password);
    Task<bool> UndoRegisterUser(string userName);
    Task<int> GetUserId(string userName);
}

public class AccountDB : IAccountDB
{
    IDbConnection dbConnection;
    MySqlCompiler compiler;
    QueryFactory queryFactory;
    IOptions<DbConfig> _dbConfig;
    ILogger<AccountDB> _logger;  

    public AccountDB(IOptions<DbConfig> dbConfig, ILogger<AccountDB> logger)
    {
        _dbConfig = dbConfig;
        _logger = logger;
        Open();
    }

    public async Task<int> GetUserId(string userName)
    {
        try
        {
            User user = await queryFactory.Query("users").Where("userName", userName).FirstAsync<User>();
            return user.UserId;
        }
        catch (Exception ex)
        {
            _logger.ZLogError(ex.Message);
            return -1;
        }
    } 

    public async Task<User?> GetUser(string userName)
    {
        User user;
        try
        {
            user = await queryFactory.Query("users").Where("userName", userName).FirstAsync<User>();
        }
        catch (Exception ex)
        {
            _logger.ZLogError(ex.Message);
            return null;
        }
        return user;
    }

    

    public async Task<bool> UndoRegisterUser(string userName)
    {
        try
        {
            await queryFactory.Query("users").Where("userName", userName).DeleteAsync();
            return true;
        }
        catch ( Exception ex )
        {
            _logger.ZLogError(ex.Message);
            return false;
        }
    }

    public async Task<int> RegisterUser(string userName, string password)
    {
        try
        {
            User user = new User
            {
                UserName = userName,
                Password = password,
                PasswordSalt = "",
                Permission = 0
            };

            user.SetSalt();
            user.SetHash();

            return await queryFactory.Query("users").InsertGetIdAsync<int>(new
            {
                userName = user.UserName,
                password = user.Password,
                passwordSalt = user.PasswordSalt,
                permission = user.Permission
            });
        }
        catch (Exception ex)
        {
            _logger.ZLogError(ex.Message);
            return -1;
        }
    }

    void Dispose()
    {
        try
        {
            dbConnection.Close();
        }
        catch( Exception ex )
        {
            _logger.ZLogError(ex.Message);
        }
    }

    void Open()
    {
        try
        {
            dbConnection = new MySqlConnection(_dbConfig.Value.MysqlAccountDb);
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
