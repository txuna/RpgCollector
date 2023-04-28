using Microsoft.Extensions.Configuration.UserSecrets;
using Microsoft.Extensions.Options;
using RpgCollector.Models;
using RpgCollector.Models.MasterData;
using RpgCollector.Utility;
using SqlKata.Compilers;
using SqlKata.Execution;
using System.Data;

namespace RpgCollector.Services
{
    public interface IPlayerAccessDB
    {
        Task<bool> CreatePlayer(int userId);
        Task<PlayerData?> GetPlayerFromUserId(int userId);
        Task<bool> AddItemToPlayer(int userId, int itemId, int quantity);
        Task<bool> AddMoneyToPlayer(int userId, int money);
        Task<bool> InsertPlayerEquipmentItem(int userId, int itemId, int quantity);
        Task<bool> InsertPlayerConsumptionItem(int userId, int itemId, int quantity);
        Task<bool> UpdatePlayerConsumptionItem(int userId, int itemId, PlayerItem playerItem, int quantity);
        Task<PlayerItem?> GetPlayerConsumptionItem(int userId, int itemId);
        Task<bool> HasItem(int userId, int itemId);
        Task<MasterItemAttribute?> GetMasterItemAttributeFromId(int attributeId);
        Task<MasterItem?> GetMasterItemFromItemId(int itemId);
    }

    public class PlayerAccessDB : IPlayerAccessDB
    {
        IDbConnection? dbConnection;
        MySqlCompiler compiler;
        QueryFactory queryFactory;

        public PlayerAccessDB(IOptions<DbConfig> dbConfig) 
        {
            dbConnection = DatabaseConnector.OpenMysql(dbConfig.Value.MysqlGameDb);
            if (dbConnection != null)
            {
                compiler = new MySqlCompiler();
                queryFactory = new QueryFactory(dbConnection, compiler);
            }
        }

        public async Task<bool> AddMoneyToPlayer(int userId, int money)
        {
            try
            {
                PlayerData? playerData = await GetPlayerFromUserId(userId);
                if (playerData == null)
                {
                    return false;
                }
                await queryFactory.Query("players").Where("userId", userId).UpdateAsync(new
                {
                    money = playerData.Money + money
                });
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        public async Task<PlayerData?> GetPlayerFromUserId(int userId)
        {
            PlayerData playerData; 
            try
            {
                playerData = await queryFactory.Query("players").Where("userId", userId).FirstAsync<PlayerData>();
            }
            catch (Exception ex)
            {
                return null; 
            }
            return playerData;
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
            catch ( Exception e)
            {
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
            catch ( Exception e )
            {
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
            catch ( Exception e)
            {
                return false;
            }
        }

        // 사용자 아이템에 아이템 기반으로 추가 어트리뷰트 - 타입 기반으로 quantity overlapp 되는지 확인
        // 만약 오버래핑 가능한것이라면 playerId, ItemId 기반으로 찾아서 quantity 늘리기 
        // 오러래핑 불가능한 경우 quantity 만큼 row 추가
        public async Task<bool> AddItemToPlayer(int userId, int itemId, int quantity)
        {
            // 돈 아이템이라면
            if (itemId == 1)
            {
                if (!await AddMoneyToPlayer(userId, quantity))
                {
                    return false;
                }
                return true;
            }
            //itemId를 기반으로 master_item_info에서 해당 아이템의 프로토타입을 불러옴
            MasterItem? masterItem = await GetMasterItemFromItemId(itemId);
            if (masterItem == null)
            {
                return false;
            }

            //해당 아이템의 프로토타입에서 attributeId를 확인하고 master_item_attribute에서 typeId를 불러옴 
            MasterItemAttribute? masterItemAttribute = await GetMasterItemAttributeFromId(masterItem.AttributeId);
            if (masterItemAttribute == null)
            {
                return false;
            }

            //typeId가 0이라면 오버래핑 허용(소비 아이템) -- 1이라면 새로운 아이템으로(장비 아이템)
            if (masterItemAttribute.TypeId == 0)
            {
                // 기존에 있다면 Update 없다면 Insert
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
                else // 새로운 아이템
                {
                    if (!await InsertPlayerConsumptionItem(userId, itemId, quantity))
                    {
                        return false;
                    }
                }
            }
            else // 장비아이템
            {
                if (!await InsertPlayerEquipmentItem(userId, itemId, quantity))
                {
                    return false;
                }
            }
            return true;
        }

        public async Task<bool> CreatePlayer(int userId)
        {
            PlayerData playerData = new PlayerData
            {
                UserId = userId,
                Hp = 0, 
                Exp = 0, 
                Level = 0, 
                Money = 0
            };
            try
            {
                await queryFactory.Query("players").InsertAsync(new
                {
                    userId = playerData.UserId,
                    hp = playerData.Hp,
                    exp = playerData.Exp,
                    level = playerData.Level,
                    money = playerData.Money
                });
            } 
            catch (Exception ex)
            {
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
}
