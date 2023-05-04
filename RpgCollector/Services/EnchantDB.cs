﻿using Microsoft.Extensions.Options;
using MySqlConnector;
using RpgCollector.Models;
using RpgCollector.Models.EnchantModel;
using RpgCollector.Models.MasterModel;
using RpgCollector.Models.PlayerModel;
using SqlKata.Compilers;
using SqlKata.Execution;
using System.Data;
using ZLogger;

namespace RpgCollector.Services;

public interface IEnchantDB
{ 
    Task<bool> DoEnchant(PlayerItem playerItem);
    Task<bool> EnchantLog(int playerItemId, int userId, int currentEnchantCount, int result);
}

public class EnchantDB : IEnchantDB
{
    IDbConnection dbConnection;
    MySqlCompiler compiler;
    QueryFactory queryFactory;
    IOptions<DbConfig> _dbConfig;
    ILogger<EnchantDB> _logger;

    public EnchantDB(IOptions<DbConfig> dbConfig, ILogger<EnchantDB> logger) 
    {
        _dbConfig = dbConfig;
        _logger = logger;
        Open();
    }

    public async Task<bool> EnchantLog(int playerItemId, int userId, int currentEnchantCount, int result)
    {
        try
        {
            await queryFactory.Query("player_enchant_log").InsertAsync(new
            {
                playerItemId = playerItemId,
                userId = userId,
                enchantCount = currentEnchantCount + result,
                result = result
            });
            return true;
        }
        catch (Exception ex)
        {
            _logger.ZLogError(ex.Message);
            return false;
        }
    }

    public async Task<bool> DoEnchant(PlayerItem playerItem)
    {
        try
        {
            await queryFactory.Query("player_items").Where("playerItemId", playerItem.PlayerItemId).UpdateAsync(new
            {
                enchantCount = playerItem.EnchantCount + 1
            });
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
