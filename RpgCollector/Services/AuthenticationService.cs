using Microsoft.Extensions.Options;
using MySqlConnector;
using RpgCollector.Models;
using RpgCollector.ResponseModels;
using SqlKata.Compilers;
using SqlKata.Execution;
using StackExchange.Redis;
using System.Data;
using System.Data.SqlTypes;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text.Json;
using System.Xml.Linq;

namespace RpgCollector.Services
{
    public interface ICustomAuthenticationService 
    {
        Task<(bool success, string content)> Login(string username, string password);
        Task<(bool success, string content)> Register(string username, string password);
        Task<(bool success, string content)> Logout(string userId);
        Task<bool> CheckAlreadyLogin(string userId, IDatabase redisDB);
        bool IsOpenDB();
        Task<bool> StoreUserInRedis(int index, string userId, string authToken, IDatabase redisDB);
    }

    public class AuthenticationService : ICustomAuthenticationService
    {
        private IOptions<DbConfig> _dbConfig;
        private QueryFactory _queryFactory;
        private IDbConnection _dbConnection;
        private MySqlCompiler _compiler;
        private ConnectionMultiplexer redisClient;

        /* 
            디비서버와 레디스 서버의 연결 오류로 인한 확인 프로퍼티
        */
        private bool is_open_database;
        private bool is_open_redis; 

        public AuthenticationService(IOptions<DbConfig> dbconfig) 
        {
            _dbConfig = dbconfig;

            OpenDatabase();
            OpenRedis();
            if (is_open_database)
            {
                _compiler = new MySqlCompiler();
                _queryFactory = new QueryFactory(_dbConnection, _compiler);
            }
        }

        /*
         * 유저의 Id와 Password를 인수로 받아 디비에 저장 
         * 0. 세션값에 IsLogin이 설정되어있다면 패스
         * 1. Id는 중복확인을 한다. 
         * 2. Password는 Salt 문자열을 만들고 해싱한다. 
         * 성공적으로 수행이 된다면 true 
         * 실패한다면 false
         */
        public async Task<(bool success, string content)> Register(string userId, string password)
        {
            if (!IsOpenDB())
            {
                return (false, "Redis Server or Mysql Database Down");
            }
            try
            {
                User user = new User
                {
                    UserId = userId,
                    Password = password,
                    PasswordSalt = "",
                    Permission = 0
                };
                user.SetupSaltAndHash();
                await _queryFactory.Query("users").InsertAsync(new
                {
                    userId = user.UserId,
                    password = user.Password,
                    passwordSalt = user.PasswordSalt,
                    permission = user.Permission
                });
            } catch (Exception ex)
            {
                return (false, "Already Exist Same User ID");
            }
            
            return (true, "Register");
        }

        /*
         * 유저의 Id와 Password를 인수로 받아 디비에서 검증을 진행한다. 
         * 0. Redis에 Auth 토큰을 저장되어 있는지 확인한다. 
         * 1. Id가 존재하는지 확인한다. 있다면 해당 User를 불러온다. 
         * 2. 넘겨받은 Password값을 디비에 저장된 PasswordSalt값과 비교한다. 
         * 3. 성공적으로 로그인이 수행된다면 Redis에 User에 대한 AuthToken을 설정한다.
         * 성공적으로 로그인이 수행된다면 true와 인증토큰을 반환한다. 
         * 실패한다면 false와 메시지 전달 
         */ 
        public async Task<(bool success, string content)> Login(string userId, string password)
        {
            IDatabase redisDB = redisClient.GetDatabase();
            User user;
            if (!IsOpenDB())
            {
                return (false, "Redis Server or Mysql Database Down");
            }
            try
            {
                user = await _queryFactory.Query("users").Where("userId", userId).FirstAsync<User>();
            }
            catch (Exception ex)
            {
                return (false, "None Exist User ID");
            }

            // 틀린 비밀번호라면
            if(user.Password != AuthenticationSupporter.GenerateHash(password, user.PasswordSalt))
            {
                return (false, "Invalid Password");
            }
            // 이미 로그인된 사용자라면
            bool success = await CheckAlreadyLogin(userId, redisDB);
            if (!success)
            {
                return (false, "Already Login UserId");
            }
            // 로그인에 성공한 User에게 Token 발급
            string authToken = AuthenticationSupporter.GenerateAuthToken();
            // 유니코드 변환 문제
            authToken = authToken.Replace("+", "d");
            // 유저정보 Redis에 저장
            success = await StoreUserInRedis(user.Index, userId, authToken, redisDB);
            if (!success)
            {
                return (false, "Redis Server Error");
            }

            UserLoginResponse userLoginResponse = new UserLoginResponse
            {
                UserId = userId,
                AuthToken = authToken,
            };
            return (true, JsonSerializer.Serialize(userLoginResponse));
        }


        /*
         * Redis에서 해당 유저의 인증을 삭제한다. 
         * 만약 성공적으로 삭제가 된다면 true 
         * 삭제가 실패한다면 false 
         */
        public async Task<(bool success, string content)> Logout(string userId)
        {
            IDatabase redisDB = redisClient.GetDatabase();
            try
            {
                await redisDB.HashDeleteAsync("Users", userId);
            }
            catch (Exception ex)
            {
                return (false, "Fail Logout");
            }
            return (true, "Success Login");
        }


        public async Task<bool> StoreUserInRedis(int index, string userId, string authToken, IDatabase redisDB)
        {
            RedisUser redisUser = new RedisUser
            {
                Index = index, 
                UserId = userId,
                AuthToken = authToken,
                State = UserState.Login,
                TimeStamp = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond
            };
            // 발급한 Token Redis에 저장
            try
            {
                await redisDB.HashSetAsync("Users", userId, JsonSerializer.Serialize(redisUser));
            }catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        /*
         * userID에 대해서 이미 만들어진 토큰이 있는지 확인하는 함수 
         * 만들어진 토큰이 있다면 false 
         * 만들어진 토큰이 없다면 true
         */
        public async Task<bool> CheckAlreadyLogin(string userId, IDatabase redisDB)
        {
            try
            {
                HashEntry[] hashEntries = await redisDB.HashGetAllAsync("Users");
                foreach (HashEntry entry in hashEntries)
                {
                    string? key = entry.Name;
                    if (key == null)
                    {
                        return false;
                    }
                    RedisUser? _redisUser = JsonSerializer.Deserialize<RedisUser>(entry.Value.ToString());
                    if (_redisUser == null)
                    {
                        return false;
                    }
                    if (_redisUser.UserId == userId)
                    {
                        return false;
                    }
                }
                return true;
            }catch (Exception ex)
            {
                return false;
            }
        }

        public bool IsOpenDB()
        {
            if(is_open_redis && is_open_database)
            {
                return true;
            }
            return false;
        }

        public void OpenDatabase()
        {
            try
            {
                _dbConnection = new MySqlConnection(_dbConfig.Value.MysqlAccountDb);
                _dbConnection.Open(); 
                is_open_database = true;
            }
            catch (Exception ex)
            {
                is_open_database = false;
            }
        }

        public void OpenRedis()
        {
            ConfigurationOptions option = new ConfigurationOptions
            {
                //AbortOnConnectFail = false,
                EndPoints = { _dbConfig.Value.RedisDb }
            };
            try
            {
                redisClient = ConnectionMultiplexer.Connect(option);
                is_open_redis = true;
            }
            catch (Exception e)
            {
                is_open_redis = false;
            }
        }

        public void CloseDatabase()
        {
            try
            {
                _dbConnection.Close();
                is_open_database = false;
            }
            catch (Exception e)
            {
                is_open_database = false;
            }
        }

        public void CloseRedis()
        {
            try
            {
                redisClient.Dispose();
                is_open_redis = false;
            }
            catch (Exception e)
            {
                is_open_redis = false;
            }
        }

        public void Dispose()
        {
            CloseDatabase();
        }
    }
}
