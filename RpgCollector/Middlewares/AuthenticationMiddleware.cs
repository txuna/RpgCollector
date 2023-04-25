using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using RpgCollector.Models;
using StackExchange.Redis;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;
using RpgCollector.RequestModels;
using System.Net.Http;
using Microsoft.Extensions.Options;

namespace RpgCollector.Middlewares
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class AuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string[] exclusivePaths = new[] { "/Login", "/Register" };
        private bool is_open_redis;
        ConnectionMultiplexer? redisClient;
        private string _redisAddress; 

        public AuthenticationMiddleware(RequestDelegate next, string redisAddress)
        {
            _next = next;
            is_open_redis = false;
            _redisAddress = redisAddress;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            OpenRedis();
            if (!is_open_redis)
            {
                httpContext.Response.StatusCode = 401;
                await httpContext.Response.WriteAsync("Redis Server Down");
                return;
            }
            // 인증을 거쳐야하는 PATH라면
            if (!CheckExclusivePath(httpContext))
            {
                // 유효한 컨테스트인지 검증
                if (!VerifyHeader(httpContext))
                {
                    httpContext.Response.StatusCode = 401;
                    await httpContext.Response.WriteAsync("Invalid Header");
                    return;
                }
                // 유효한 UserId와 AuthToken인지 확인 
                if (!VerifyToken(httpContext))
                {
                    httpContext.Response.StatusCode = 401;
                    await httpContext.Response.WriteAsync("Invalid Token");
                    return;
                }
                if (!VerifyVersion())
                {
                    httpContext.Response.StatusCode = 401;
                    await httpContext.Response.WriteAsync("Invalid Data Version");
                    return;
                }
            }
            CloseRedis();
            await _next(httpContext);
        }

        /*
         * AuthToken 및 userId의 검증을 거쳐야 하는 PATH인지 확인한다. 
         * 인증과정을 거쳐야 하는 PATH라면 false, 예외된 PATH라면 true
         */
        public bool CheckExclusivePath(HttpContext httpContext)
        {
            string rpath = httpContext.Request.Path;
            foreach (var path in exclusivePaths)
            {
                if (rpath.Contains(path))
                {
                    return true;
                }
            }
            return false;
        }

        /*
         * 요청 헤더가 정상적인 값을 포함하고 있는지를 검증한다. 
         */
        public bool VerifyHeader(HttpContext httpContext)
        {
            if (!httpContext.Request.Headers.ContainsKey("User-Id"))
            {
                return false; 
            }
            if (!httpContext.Request.Headers.ContainsKey("Auth-Token"))
            {
                return false; 
            }
            string? authToken = httpContext.Request.Headers["Auth-Token"];
            string? userId = httpContext.Request.Headers["User-Id"];
            // 둘다 빈값이라면
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(authToken))
            {
                return false; 
            }

            return true;
        }

        /*
         * 전송받은 UserId와 AuthToken이 Redis에 저장되어 있는지 저장되어있다면 올바른 토큰인지 확인한다.  
         */
        public bool VerifyToken(HttpContext httpContext)
        {
            string? authToken = httpContext.Request.Headers["Auth-Token"];
            string? userId = httpContext.Request.Headers["User-Id"];
            IDatabase redisDB = redisClient.GetDatabase();
            try
            {
                // userId가 존재한다면 해당 userId를 불러와서 token 비교
                if (!redisDB.HashExists("Users", userId))
                {
                    return false;
                }

                RedisValue content = redisDB.HashGet("Users", userId);
                RedisUser? redisUser = JsonSerializer.Deserialize<RedisUser>(content.ToString());

                if (redisUser == null)
                {
                    return false;
                }
                // 저장된 토큰이랑 불일치
                if (redisUser.AuthToken != authToken)
                {
                    return false;
                }
                // 유효한 인증이라면 
                long timeStamp = TimeSupporter.GetTimeStamp();
                if (redisUser == null)
                {
                    return false;
                }
                // 최신 timestamp 넣고 재설정
                redisUser.TimeStamp = timeStamp;
                redisDB.HashSet("Users", userId, JsonSerializer.Serialize(redisUser));
                return true;
            }catch (Exception ex)
            {
                return false;
            }
        }

        /*
         * 마스터 데이터와 클라이언트의 버전을 확인하는 로직
         */
        public bool VerifyVersion()
        {
            return true;
        }

        public void OpenRedis()
        {
            ConfigurationOptions option = new ConfigurationOptions
            {
                EndPoints = { _redisAddress }
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
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class AuthenticationMiddlewareExtensions
    {
        public static IApplicationBuilder UseAuthenticationMiddleware(this IApplicationBuilder builder, string redisAddress)
        {
            return builder.UseMiddleware<AuthenticationMiddleware>(redisAddress);
        }
    }
}
