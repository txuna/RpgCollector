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
    Task<PlayerState?> GetPlayerFromUserId(int userId);
    Task<bool> AddItemToPlayer(int userId, int itemId, int quantity);
    Task<bool> AddMoneyToPlayer(int userId, int money);
    Task<bool> InsertPlayerEquipmentItem(int userId, int itemId, int quantity);
    Task<bool> InsertPlayerConsumptionItem(int userId, int itemId, int quantity);
    Task<bool> UpdatePlayerConsumptionItem(int userId, int itemId, PlayerItem playerItem, int quantity);
    Task<PlayerItem?> GetPlayerConsumptionItem(int userId, int itemId);
    Task<bool> HasItem(int userId, int itemId);
    Task<bool> IsItemOwner(int playerItemId, int userId);
    Task<PlayerItem?> GetPlayerItem(int playerItemId);
    Task<bool> SetInitPlayerState(int userId);
    Task<bool> SetInitPlayerItems(int userId);
    Task<bool> RemovePlayerItem(int playerItemId);
    Task<int> GetPlayerMoney(int userId);
    Task<PlayerItem[]?> GetPlayerAllItems(int userId);
    Task<bool> UndoCreatePlayer(int userId);
}

public class PlayerAccessDB : IPlayerAccessDB
{
    IDbConnection? dbConnection;
    MySqlCompiler compiler;
    QueryFactory queryFactory;
    IOptions<DbConfig> _dbConfig;
    ILogger<PlayerAccessDB> _logger;
    IMasterDataDB _masterDataDB;

    public PlayerAccessDB(IOptions<DbConfig> dbConfig, ILogger<PlayerAccessDB> logger, IMasterDataDB masterDataDB) 
    {
        _dbConfig = dbConfig;
        _logger = logger;
        _masterDataDB = masterDataDB;
        Open();
    }

    public async Task<bool> UndoCreatePlayer(int userId)
    {
        try
        {
            await queryFactory.Query("players").Where("userId", userId).DeleteAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.ZLogError(ex.Message);
            return false;
        }
    }

    public async Task<PlayerItem[]?> GetPlayerAllItems(int userId)
    {
        try
        {
            IEnumerable<PlayerItem> eitems = await queryFactory.Query("player_items").Where("userId", userId).GetAsync<PlayerItem>();
            PlayerItem[] items = eitems.ToArray();
            return items;
        }
        catch (Exception ex)
        {
            _logger.ZLogError(ex.Message);
            return null;
        }
    }

    public async Task<int> GetPlayerMoney(int userId)
    {
        try
        {
            PlayerState playerState = await queryFactory.Query("players").Where("userId", userId).FirstAsync<PlayerState>();    
            return playerState.Money;
        }
        catch (Exception ex)
        {
            _logger.ZLogError(ex.Message);
            return 0;
        }
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

    public async Task<bool> AddMoneyToPlayer(int userId, int money)
    {
        try
        {
            PlayerState? playerState = await GetPlayerFromUserId(userId);
            if (playerState == null)
            {
                return false;
            }
            await queryFactory.Query("players").Where("userId", userId).UpdateAsync(new
            {
                money = playerState.Money + money
            });
        }
        catch (Exception ex)
        {
            _logger.ZLogError(ex.Message);
            return false;
        }
        return true;
    }

    public async Task<PlayerState?> GetPlayerFromUserId(int userId)
    {
        PlayerState playerState; 
        try
        {
            playerState = await queryFactory.Query("players").Where("userId", userId).FirstAsync<PlayerState>();
        }
        catch (Exception ex)
        {
            _logger.ZLogError(ex.Message);
            return null; 
        }
        return playerState;
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

    public async Task<bool> IsItemOwner(int playerItemId, int userId)
    {
        try
        {
            int count = await queryFactory.Query("player_items").Where("playerItemid", playerItemId)
                                                          .Where("userId", userId)
                                                          .CountAsync<int>();
            if (count < 1)
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
        MasterItem? masterItem = _masterDataDB.GetMasterItem(itemId);

        if (masterItem == null)
        {
            return false;
        }

        MasterItemAttribute? masterItemAttribute = _masterDataDB.GetMasterItemAttribute(masterItem.AttributeId);
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
            InitPlayerState initPlayerState = _masterDataDB.GetInitPlayerState();
            await queryFactory.Query("players").InsertAsync(new
            {
                userId = userId,
                hp = initPlayerState.Hp,
                exp = initPlayerState.Exp,
                level = initPlayerState.Level,
                money = initPlayerState.Money,
                attack = initPlayerState.Attack, 
                defence = initPlayerState.Defence,
                magic = initPlayerState.Magic,
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
            InitPlayerItem[] initPlayerItem = _masterDataDB.GetInitPlayerItems();
            foreach(var item in initPlayerItem)
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
