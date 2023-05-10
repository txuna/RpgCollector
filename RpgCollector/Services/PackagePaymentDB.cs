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
    Task<bool> ReportReceipt(int receiptId, int packageId, int userId);
    Task<bool> UndoReportPackage(int receiptId);
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

    public async Task<bool> UndoReportPackage(int receiptId)
    {
        try
        {
            int effectedRow = await queryFactory.Query("player_payment_info")
                                                .Where("receiptId", receiptId)
                                                .DeleteAsync();
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

    public async Task<bool> ReportReceipt(int receiptId, int packageId, int userId)
    {
        try
        {
            int effectedRow = await queryFactory.Query("player_payment_info").InsertAsync(new
            {
                receiptId = receiptId,
                userId = userId,
                packageId = packageId, 
            });

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

    public async Task<bool> VerifyReceipt(int receiptId)
    {
        try
        {
            int count = await queryFactory.Query("player_payment_info").Where("receiptId", receiptId).CountAsync<int>();

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
