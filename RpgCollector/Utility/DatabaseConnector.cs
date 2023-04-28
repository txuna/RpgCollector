using MySqlConnector;
using StackExchange.Redis;
using System.Data;

namespace RpgCollector.Utility
{
    public static class DatabaseConnector
    {
        public static IDbConnection? OpenMysql(string connectString)
        {
            IDbConnection dbConnection;
            try
            {
                dbConnection = new MySqlConnection(connectString);
                dbConnection.Open();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
            return dbConnection;
        }

        public static void CloseMysql(this IDbConnection dbConnection)
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

        public static ConnectionMultiplexer? OpenRedis(string connectString)
        {
            ConnectionMultiplexer redisClient;

            ConfigurationOptions option = new ConfigurationOptions
            {
                EndPoints = { connectString }
            };

            try
            {
                redisClient = ConnectionMultiplexer.Connect(option);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }

            return redisClient;
        }

        public static void CloseRedis(this ConnectionMultiplexer redisClient)
        {
            try
            {
                redisClient.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
