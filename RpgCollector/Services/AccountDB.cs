using Microsoft.Extensions.Options;
using RpgCollector.Models;
using RpgCollector.Utility;
using SqlKata.Compilers;
using SqlKata.Execution;
using StackExchange.Redis;
using System.Data;
using System.Text.Json;

namespace RpgCollector.Services;

public interface IAccountDB
{
    bool VerifyPassword(User user, string requestPassword);
    Task<User?> GetUser(string userName);
    Task<bool> RegisterUser(string userName, string password);
    Task<bool> UndoRegisterUser(string userName);
    Task<int> GetUserId(string userName);
}

public class AccountDB : IAccountDB
{
    IDbConnection dbConnection;
    MySqlCompiler compiler;
    QueryFactory queryFactory;


    public AccountDB(IOptions<DbConfig> dbConfig)
    {
        dbConnection = DatabaseConnector.OpenMysql(dbConfig.Value.MysqlAccountDb);
        if(dbConnection != null) 
        {
            compiler = new MySqlCompiler(); 
            queryFactory = new QueryFactory(dbConnection, compiler);
        }
    }

    public async Task<int> GetUserId(string userName)
    {
        try
        {
            int userId = await queryFactory.Query("users").Select("userId").Where("userName", userName).FirstAsync<int>();
            return userId;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
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
            Console.WriteLine(ex.Message);
            return null;
        }
        return user;
    }

    public bool VerifyPassword(User user, string requestPassword)
    {
        if (user.Password != HashManager.GenerateHash(requestPassword, user.PasswordSalt))
        {
            return false; 
        }
        return true;
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
            Console.WriteLine(ex.Message);
            return false;
        }
    }

    public async Task<bool> RegisterUser(string userName, string password)
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

            await queryFactory.Query("users").InsertAsync(new
            {
                userName = user.UserName,
                password = user.Password,
                passwordSalt = user.PasswordSalt,
                permission = user.Permission
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return false;
        }

        return true;
    }

    void Dispose()
    {
        if(dbConnection != null)
        {
            dbConnection.CloseMysql();
        }
    }
}
