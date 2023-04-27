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
        Task<bool> AddItem(int userId, int itemId, int quantity);
    }

    public class PlayerAccessDB : IPlayerAccessDB
    {
        IDbConnection dbConnection;
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

        // 사용자 아이템에 아이템 기반으로 추가 어트리뷰트 - 타입 기반으로 quantity overlapp 되는지 확인
        // 만약 오버래핑 가능한것이라면 playerId, ItemId 기반으로 찾아서 quantity 늘리기 
        // 오러래핑 불가능한 경우 quantity 만큼 row 추가
        public async Task<bool> AddItem(int userId, int itemId, int quantity)
        {
            try
            {
                //itemId를 기반으로 master_item_info에서 해당 아이템의 프로토타입을 불러옴
                MasterItem masterItem = await queryFactory.Query("master_item_info")
                                                          .Where("itemId", itemId)
                                                          .FirstAsync<MasterItem>();

                //해당 아이템의 프로토타입에서 attributeId를 확인하고 master_item_attribute에서 typeId를 불러옴 
                MasterItemAttribute masterItemAttribute = await queryFactory.Query("master_item_attribute")
                                                        .Where("attributeId", masterItem.AttributeId)
                                                        .FirstAsync<MasterItemAttribute>();

                //typeId가 0이라면 오버래핑 허용 1이라면 새로운 아이템으로
                if (masterItemAttribute.TypeId == 1)
                {
                    // 기존에 있다면 Update 없다면 Insert
                    int count = await queryFactory.Query("player_items").Where("userId", userId).Where("itemId", itemId).CountAsync<int>();
                    // 기존에 있는 아이템
                    if (count > 0)
                    {
                        PlayerItem playerItem = await queryFactory.Query("player_items")
                                                                  .Where("userId", userId)
                                                                  .Where("itemId", itemId)
                                                                  .FirstAsync<PlayerItem>();

                        await queryFactory.Query("player_items")
                                          .Where("userId", userId)
                                          .Where("itemId", itemId)
                                          .UpdateAsync(new
                                          {
                                              quantity = playerItem.Quantity + quantity
                                          });
                    }
                    else // 새로운 아이템
                    {
                        await queryFactory.Query("player_items").InsertAsync(new
                        {
                            userId = userId,
                            itemId = itemId,
                            quantity = quantity,
                            enchantCount = 0
                        });
                    }
                }
                else // 장비아이템
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
                }
            } 
            catch (Exception ex)
            {
                return false;
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
