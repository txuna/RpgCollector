using Microsoft.Extensions.Configuration.UserSecrets;
using Microsoft.Extensions.Options;
using MySqlConnector;
using RpgCollector.Models;
using SqlKata.Compilers;
using SqlKata.Execution;
using System.Data;
using ZLogger;

namespace RpgCollector.Services;

public interface IPackagePaymentDB
{
    Task<bool> VerifyReceipt(int receiptId);
    Task<bool> BuyPackage(int receiptId, int packageId, int userId);
}

public class PackagePaymentDB : IPackagePaymentDB
{
    IOptions<DbConfig> _dbConfig;
    IDbConnection dbConnection;
    MySqlCompiler compiler;
    QueryFactory queryFactory;
    ILogger<PackagePaymentDB> _logger;

    public PackagePaymentDB(IOptions<DbConfig> dbConfig, ILogger<PackagePaymentDB> logger) 
    {
        _dbConfig = dbConfig;
        _logger = logger;
        Open();
    }

    public async Task<bool> BuyPackage(int receiptId, int packageId, int userId)
    {
        try
        {
            await queryFactory.Query("player_payment_log").InsertAsync(new
            {
                receiptId = receiptId,
                userId = userId,
                packageId = packageId, 
            });
            return true;
        }
        catch (Exception ex)
        {
            _logger.ZLogError(ex.Message);
            return false;
        }
    }

    public async Task<bool> VerifyReceipt(int receiptId)
    {
        try
        {
            int count = await queryFactory.Query("player_payment_log").Where("receiptId", receiptId).CountAsync<int>();
            if (count > 0)
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
