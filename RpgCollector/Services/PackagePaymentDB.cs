using Microsoft.Extensions.Configuration.UserSecrets;
using Microsoft.Extensions.Options;
using MySqlConnector;
using RpgCollector.Models;
using RpgCollector.Models.PackgeItemData;
using SqlKata.Compilers;
using SqlKata.Execution;
using System.Data;
using ZLogger;

namespace RpgCollector.Services;

public interface IPackagePaymentDB
{
    Task<bool> VerifyReceipt(int receiptId);
    Task<PackageItem[]?> GetPackageItems(int packageId);
    Task<bool> BuyPackage(int receiptId, int packageId, int userId);
    Task<bool> VertifyPackageId(int packageId);
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
            await queryFactory.Query("player_payment_info").InsertAsync(new
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

    public async Task<bool> VertifyPackageId(int packageId)
    {
        try
        {
            int count = await queryFactory.Query("master_package_info").Where("packageId", packageId).CountAsync<int>();
            if(count < 1)
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

    public async Task<PackageItem[]?> GetPackageItems(int packageId)
    {
        try
        {
            IEnumerable<PackageItem> items = await queryFactory.Query("master_package_info").Where("packageId", packageId).GetAsync<PackageItem>();
            PackageItem[] packageItem = items.ToArray();
            return packageItem;
        }
        catch (Exception ex)
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
