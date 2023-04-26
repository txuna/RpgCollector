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
        Task<(bool success, string content)> Login(string userName, string password);
        Task<(bool success, string content)> Register(string userName, string password);
        Task<(bool success, string content)> Logout(string userName);
        Task<bool> CheckAlreadyLogin(string userName, IDatabase redisDB);
        bool IsOpenDB();
        Task<bool> StoreUserInRedis(int userId, string userName, string authToken, IDatabase redisDB);
    }

    public class AuthenticationService : ICustomAuthenticationService
    {
        private IDbConnection? _dbConnection;
        private ConnectionMultiplexer? redisClient;

        private MySqlCompiler _compiler;
        private QueryFactory _queryFactory;


        public AuthenticationService(IOptions<DbConfig> dbConfig) 
        {
            _dbConnection = DatabaseSuppoter.OpenMysql(dbConfig.Value.MysqlAccountDb);
            redisClient = DatabaseSuppoter.OpenRedis(dbConfig.Value.RedisDb);

            if (IsOpenDB())
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
        public async Task<(bool success, string content)> Register(string userName, string password)
        {
            if (!IsOpenDB())
            {
                return (false, "Database Connection Failed");
            }
            try
            {
                User user = new User
                {
                    UserName = userName,
                    Password = password,
                    PasswordSalt = "",
                    Permission = 0
                };
                user.SetupSaltAndHash();
                await _queryFactory.Query("users").InsertAsync(new
                {
                    userName = user.UserName,
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
        public async Task<(bool success, string content)> Login(string userName, string password)
        {
            if(!IsOpenDB())
            {
                return (false, "DB Connection Failed");
            }
            IDatabase redisDB = redisClient.GetDatabase();
            User user;
            try
            {
                user = await _queryFactory.Query("users").Where("userName", userName).FirstAsync<User>();
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
            bool success = await CheckAlreadyLogin(userName, redisDB);
            if (!success)
            {
                return (false, "Already Login UserName");
            }
            // 로그인에 성공한 User에게 Token 발급
            string authToken = AuthenticationSupporter.GenerateAuthToken();
            // 유니코드 변환 문제
            authToken = authToken.Replace("+", "d");
            // 유저정보 Redis에 저장
            success = await StoreUserInRedis(user.UserId, userName, authToken, redisDB);
            if (!success)
            {
                return (false, "Redis Server Error");
            }

            UserLoginResponse userLoginResponse = new UserLoginResponse
            {
                UserName = userName,
                AuthToken = authToken,
            };
            Console.WriteLine(userLoginResponse);
            return (true, JsonSerializer.Serialize(userLoginResponse));
        }


        /*
         * Redis에서 해당 유저의 인증을 삭제한다. 
         * 만약 성공적으로 삭제가 된다면 true 
         * 삭제가 실패한다면 false 
         */
        public async Task<(bool success, string content)> Logout(string userName)
        {
            IDatabase redisDB = redisClient.GetDatabase();
            try
            {
                await redisDB.HashDeleteAsync("Users", userName);
            }
            catch (Exception ex)
            {
                return (false, "Fail Logout");
            }
            return (true, "Success Login");
        }


        public async Task<bool> StoreUserInRedis(int userId, string userName, string authToken, IDatabase redisDB)
        {
            RedisUser redisUser = new RedisUser
            {
                UserId = userId, 
                UserName = userName,
                AuthToken = authToken,
                State = UserState.Login,
                TimeStamp = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond
            };
            // 발급한 Token Redis에 저장
            try
            {
                await redisDB.HashSetAsync("Users", userName, JsonSerializer.Serialize(redisUser));
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
        public async Task<bool> CheckAlreadyLogin(string userName, IDatabase redisDB)
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
                    if (_redisUser.UserName == userName)
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
            if(redisClient != null && _dbConnection != null)
            {
                return true;
            }
            return false;
        }

        public void Dispose()
        {
            if(redisClient != null)
            {
                redisClient.CloseRedis();
            }
            if(_dbConnection != null)
            {
                _dbConnection.CloseMysql();
            }
        }
    }
}
