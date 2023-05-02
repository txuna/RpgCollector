using Microsoft.Extensions.Configuration.UserSecrets;
using Microsoft.Extensions.Options;
using MySqlConnector;
using RpgCollector.Models;
using RpgCollector.Models.InitPlayerModel;
using RpgCollector.Models.MasterModel;
using RpgCollector.Models.PlayerModel;
using RpgCollector.Utility;
using SqlKata.Compilers;
using SqlKata.Execution;
using System.Data;
using ZLogger;

namespace RpgCollector.Services;

public interface IPlayerAccessDB
{
    Task<TypeDefinition> GetItemType(int attributeId);
    Task<PlayerInfo?> GetPlayerFromUserId(int userId);
    Task<bool> AddItemToPlayer(int userId, int itemId, int quantity);
    Task<bool> AddMoneyToPlayer(int userId, int money);
    Task<bool> InsertPlayerEquipmentItem(int userId, int itemId, int quantity);
    Task<bool> InsertPlayerConsumptionItem(int userId, int itemId, int quantity);
    Task<bool> UpdatePlayerConsumptionItem(int userId, int itemId, PlayerItem playerItem, int quantity);
    Task<PlayerItem?> GetPlayerConsumptionItem(int userId, int itemId);
    Task<bool> HasItem(int userId, int itemId);
    Task<MasterItemAttribute?> GetMasterItemAttributeFromId(int attributeId);
    Task<MasterItem?> GetMasterItemFromItemId(int itemId);
    Task<PlayerItem?> GetPlayerItem(int playerItemId);
    Task<bool> SetInitPlayerState(int userId);
    Task<bool> SetInitPlayerItems(int userId);
    Task<bool> RemovePlayerItem(int playerItemId);
    // 착용한 아이템일 기반으로 플레이어의 공격력, 방어력, 마법력을 가지고옴 - 강화 수치 고려
    //void GetPlayerState();
}

public class PlayerAccessDB : IPlayerAccessDB
{
    IDbConnection? dbConnection;
    MySqlCompiler compiler;
    QueryFactory queryFactory;
    IOptions<DbConfig> _dbConfig;
    ILogger<PlayerAccessDB> _logger;

    public PlayerAccessDB(IOptions<DbConfig> dbConfig, ILogger<PlayerAccessDB> logger) 
    {
        _dbConfig = dbConfig;
        _logger = logger;
        Open();
    }

    public async Task<bool> RemovePlayerItem(int playerItemId)
    {
        try
        {
            await queryFactory.Query("player_items").Where("playerItemId", playerItemId).DeleteAsync();
            return true;
        }
        catch(Exception ex)
        {
            _logger.ZLogError(ex.Message);
            return false;
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
            _logger.ZLogError(ex.Message);
            return TypeDefinition.UNKNOWN;
        }
    }

    public async Task<bool> AddMoneyToPlayer(int userId, int money)
    {
        try
        {
            PlayerInfo? playerInfo = await GetPlayerFromUserId(userId);
            if (playerInfo == null)
            {
                return false;
            }
            await queryFactory.Query("players").Where("userId", userId).UpdateAsync(new
            {
                money = playerInfo.Money + money
            });
        }
        catch (Exception ex)
        {
            _logger.ZLogError(ex.Message);
            return false;
        }
        return true;
    }

    public async Task<PlayerInfo?> GetPlayerFromUserId(int userId)
    {
        PlayerInfo playerInfo; 
        try
        {
            playerInfo = await queryFactory.Query("players").Where("userId", userId).FirstAsync<PlayerInfo>();
        }
        catch (Exception ex)
        {
            _logger.ZLogError(ex.Message);
            return null; 
        }
        return playerInfo;
    }

    public async Task<PlayerItem?> GetPlayerItem(int playerItemId)
    {
        try
        {
            PlayerItem? playerItem = await queryFactory.Query("player_items").Where("playerItemId", playerItemId).FirstAsync<PlayerItem>();
            return playerItem; ;
        }
        catch (Exception ex)
        {
            _logger.ZLogError(ex.Message);
            return null;
        }
    }

    public async Task<MasterItem?> GetMasterItemFromItemId(int itemId)
    {
        try
        {
            MasterItem masterItem = await queryFactory.Query("master_item_info")
                                                      .Where("itemId", itemId)
                                                      .FirstAsync<MasterItem>();
            return masterItem;
        }
        catch ( Exception ex )
        {
            _logger.ZLogError(ex.Message);
            return null;
        }
    }

    public async Task<MasterItemAttribute?> GetMasterItemAttributeFromId(int attributeId)
    {
        try
        {
            MasterItemAttribute masterItemAttribute = await queryFactory.Query("master_item_attribute")
                                                    .Where("attributeId", attributeId)
                                                    .FirstAsync<MasterItemAttribute>();
            return masterItemAttribute; 
        }
        catch ( Exception ex)
        {
            _logger.ZLogError(ex.Message);
            return null;
        }
    }

    public async Task<bool> HasItem(int userId, int itemId)
    {
        try
        {
            int count = await queryFactory.Query("player_items")
                                      .Where("userId", userId)
                                      .Where("itemId", itemId)
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

    public async Task<PlayerItem?> GetPlayerConsumptionItem(int userId, int itemId)
    {
        try
        {
            PlayerItem playerItem = await queryFactory.Query("player_items")
                                                              .Where("userId", userId)
                                                              .Where("itemId", itemId)
                                                              .FirstAsync<PlayerItem>();

            return playerItem;
        }
        catch ( Exception ex)
        {
            _logger.ZLogError(ex.Message);
            return null;
        }
    }

    public async Task<bool> UpdatePlayerConsumptionItem(int userId, int itemId, PlayerItem playerItem, int quantity)
    {
        try
        {
            await queryFactory.Query("player_items")
                                      .Where("userId", userId)
                                      .Where("itemId", itemId)
                                      .UpdateAsync(new
                                      {
                                          quantity = playerItem.Quantity + quantity
                                      });
            return true;
        }
        catch (Exception ex)
        {
            _logger.ZLogError(ex.Message);
            return false;
        }
    }

    public async Task<bool> InsertPlayerConsumptionItem(int userId, int itemId, int quantity)
    {
        try
        {
            await queryFactory.Query("player_items").InsertAsync(new
            {
                userId = userId,
                itemId = itemId,
                quantity = quantity,
                enchantCount = 0
            });

            return true;
        }
        catch ( Exception ex)
        {
            _logger.ZLogError(ex.Message);
            return false;
        }
    }

    public async Task<bool> InsertPlayerEquipmentItem(int userId, int itemId, int quantity)
    {
        try
        {
            for (int i = 0; i < quantity; i++)
            {
                await queryFactory.Query("player_items").InsertAsync(new
                {
                    userId = userId,
                    itemId = itemId,
                    quantity = 1,
                    enchantCount = 0
                });
            }
            return true;
        }
        catch ( Exception ex)
        {
            _logger.ZLogError(ex.Message);
            return false;
        }
    }

    // 사용자 아이템에 아이템 기반으로 추가 어트리뷰트 - 타입 기반으로 quantity overlapp 되는지 확인
    // 만약 오버래핑 가능한것이라면 playerId, ItemId 기반으로 찾아서 quantity 늘리기 
    // 오버래핑 불가능한 경우 quantity 만큼 row 추가
    public async Task<bool> AddItemToPlayer(int userId, int itemId, int quantity)
    {
        MasterItem? masterItem = await GetMasterItemFromItemId(itemId);
        if (masterItem == null)
        {
            return false;
        }

        MasterItemAttribute? masterItemAttribute = await GetMasterItemAttributeFromId(masterItem.AttributeId);
        if (masterItemAttribute == null)
        {
            return false;
        }

        if(masterItemAttribute.TypeId == (int)TypeDefinition.MONEY)
        {
            if (!await AddMoneyToPlayer(userId, quantity))
            {
                return false;
            }
            return true;
        }

        else if (masterItemAttribute.TypeId == (int)TypeDefinition.CONSUMPTION)
        {
            if (await HasItem(userId, itemId))
            {

                PlayerItem? playerItem = await GetPlayerConsumptionItem(userId, itemId);
                if (playerItem == null)
                {
                    return false;
                }
                if (!await UpdatePlayerConsumptionItem(userId, itemId, playerItem, quantity))
                {
                    return false;
                }
            }
            else
            {
                if (!await InsertPlayerConsumptionItem(userId, itemId, quantity))
                {
                    return false;
                }
            }
        }
        else if(masterItemAttribute.TypeId == (int)TypeDefinition.EQUIPMENT)
        {
            if (!await InsertPlayerEquipmentItem(userId, itemId, quantity))
            {
                return false;
            }
        }
        return true;
    }

    public async Task<bool> SetInitPlayerState(int userId)
    {
        try
        {
            InitPlayerState? initPlayerState = await queryFactory.Query("init_player_state").FirstAsync<InitPlayerState>();
            await queryFactory.Query("players").InsertAsync(new
            {
                userId = userId,
                hp = initPlayerState.Hp,
                exp = initPlayerState.Exp,
                level = initPlayerState.Level,
                money = initPlayerState.Money
            });
            return true;
        }
        catch (Exception ex)
        {
            _logger.ZLogError(ex.Message);
            return false;
        }
    }

    public async Task<bool> SetInitPlayerItems(int userId)
    {
        try
        {
            IEnumerable<InitPlayerItem> initPlayerItems = await queryFactory.Query("init_player_items").GetAsync<InitPlayerItem>();
            foreach(var item in initPlayerItems)
            {
                if(!await AddItemToPlayer(userId, item.ItemId, item.Quantity))
                {
                    return false; 
                }
            }
            return true;
        }
        catch ( Exception ex )
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
