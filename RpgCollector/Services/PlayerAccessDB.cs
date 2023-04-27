using Microsoft.Extensions.Configuration.UserSecrets;
using Microsoft.Extensions.Options;
using RpgCollector.Models;
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
