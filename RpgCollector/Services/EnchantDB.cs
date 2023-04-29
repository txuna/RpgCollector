using Microsoft.Extensions.Options;
using MySqlConnector;
using RpgCollector.Models;
using RpgCollector.Models.MasterData;
using SqlKata.Compilers;
using SqlKata.Execution;
using System.Data;

namespace RpgCollector.Services;

public interface IEnchantDB
{
    Task<bool> IsUserHasItem(int playerItemId, int userId);
    Task<MasterItem?> GetMasterItem(int itemId);
    Task<PlayerItem?> GetPlayerItem(int playerItemId);
    Task<TypeDefinition> GetItemType(int attributeId);
    Task<bool> DoEnchant(int playerItemId);
}

public class EnchantDB : IEnchantDB
{
    IDbConnection dbConnection;
    MySqlCompiler compiler;
    QueryFactory queryFactory;
    IOptions<DbConfig> _dbConfig;

    public EnchantDB(IOptions<DbConfig> dbConfig) 
    {
        _dbConfig = dbConfig;
        Open();
    }

    public async Task<bool> DoEnchant(int playerItemId)
    {

    }

    public async Task<bool> IsUserHasItem(int playerItemId, int userId)
    {
        try
        {
            int count = await queryFactory.Query("player_items").Where("playerItemid", playerItemId)
                                                          .Where("userId", userId)
                                                          .CountAsync<int>();
            if( count < 1)
            {
                return false;
            }
            return true;
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex.Message);
            return false;
        }
    }

    public async Task<MasterItem?> GetMasterItem(int itemId)
    {
        try
        {
            MasterItem? masterItem = await queryFactory.Query("master_item_info").Where("itemId", itemId).FirstAsync<MasterItem>();
            if (masterItem == null)
            {
                return null;
            }
            return masterItem;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return null;
        }
    }

    public async Task<PlayerItem?> GetPlayerItem(int playerItemId)
    {
        try
        {
            PlayerItem? playerItem = await queryFactory.Query("player_items").Where("playerItemId", playerItemId).FirstAsync<PlayerItem>();
            return playerItem;
        }
        catch( Exception ex )
        {
            Console.WriteLine(ex.Message);
            return null;
        }
    }

    public async Task<TypeDefinition> GetItemType(int attributeId)
    {
        try
        {
            MasterItemAttribute? masterItemAttribute = await queryFactory.Query("master_item_attribute")
                                                                        .Where("attributeId", attributeId)
                                                                        .FirstAsync<MasterItemAttribute>();
            if (masterItemAttribute == null)
            {
                return TypeDefinition.UNKNOWN;
            }
            return (TypeDefinition)masterItemAttribute.TypeId;

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return TypeDefinition.UNKNOWN; ;
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
            Console.WriteLine(ex.Message);
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
            Console.WriteLine(ex.Message);
        }
    }
}
