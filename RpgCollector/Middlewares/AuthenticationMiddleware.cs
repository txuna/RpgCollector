using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using RpgCollector.Models;
using StackExchange.Redis;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.Extensions.Options;
using RpgCollector.Utility;

namespace RpgCollector.Middlewares
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class AuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string[] exclusivePaths = new[] { "/Login", "/Register" };
        ConnectionMultiplexer? redisClient;
        private string _redisAddress;
        IDatabase redisDB;

        public AuthenticationMiddleware(RequestDelegate next, string redisAddress)
        {
            _next = next;
            _redisAddress = redisAddress;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            Open();
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
                if (!await VerifyToken(httpContext))
                {
                    httpContext.Response.StatusCode = 401;
                    await httpContext.Response.WriteAsync("Invalid Token");
                    return;
                }

                if (!await VerifyVersion(httpContext))
                {
                    httpContext.Response.StatusCode = 401;
                    await httpContext.Response.WriteAsync("Invalid Data Version");
                    return;
                }
            }

            redisClient.Dispose();
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
            if (!httpContext.Request.Headers.ContainsKey("User-Name"))
            {
                return false; 
            }

            if (!httpContext.Request.Headers.ContainsKey("Auth-Token"))
            {
                return false; 
            }

            if (!httpContext.Request.Headers.ContainsKey("Client-Version"))
            {
                return false;
            }

            if (!httpContext.Request.Headers.ContainsKey("MasterData-Version"))
            {
                return false;
            }

            string requestClientVersion = httpContext.Request.Headers["Client-Version"];
            string requestMasterDataVersion = httpContext.Request.Headers["MasterData-Version"];
            string? authToken = httpContext.Request.Headers["Auth-Token"];
            string? userName = httpContext.Request.Headers["User-Name"];

            // 둘다 빈값이라면
            if (string.IsNullOrEmpty(userName) 
                || string.IsNullOrEmpty(authToken) 
                || string.IsNullOrEmpty(requestClientVersion) 
                || string.IsNullOrEmpty(requestMasterDataVersion))
            {
                return false; 
            }

            return true;
        }

        /*
         * 전송받은 UserId와 AuthToken이 Redis에 저장되어 있는지 저장되어있다면 올바른 토큰인지 확인한다.  
         */
        public async Task<bool> VerifyToken(HttpContext httpContext)
        {
            string? authToken = httpContext.Request.Headers["Auth-Token"];
            string? userName = httpContext.Request.Headers["User-Name"];
            IDatabase redisDB = redisClient.GetDatabase();

            try
            {
                // userId가 존재한다면 해당 userId를 불러와서 token 비교
                if(!await redisDB.KeyExistsAsync(userName))
                {
                    return false;
                }

                string redisAuthToken = await redisDB.StringGetAsync(userName);

                // 저장된 토큰이랑 불일치
                if (redisAuthToken != authToken)
                {
                    return false;
                }
                return true;

            }catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        /*
         * 마스터 데이터와 클라이언트의 버전을 확인하는 로직
         * Redis에서 검증
         */
        public async Task<bool> VerifyVersion(HttpContext httpContext)
        {
            if (redisClient == null)
            {
                return false;
            }

            IDatabase? redisDB = redisClient.GetDatabase();
            
            string? requestClientVersion = httpContext.Request.Headers["Client-Version"];
            string? requestMasterDataVersion = httpContext.Request.Headers["MasterData-Version"];

            string? redisClientVersion = await redisDB.StringGetAsync("ClientVersion");
            string? redisMasterDataVersion = await redisDB.StringGetAsync("MasterDataVersion"); 

            if(requestClientVersion != redisClientVersion)
            {
                return false; 
            }

            if(requestMasterDataVersion != redisMasterDataVersion)
            {
                return false;
            }

            return true;
        }

        void Open()
        {
            ConfigurationOptions option = new ConfigurationOptions
            {
                EndPoints = { _redisAddress }
            };
            try
            {
                redisClient = ConnectionMultiplexer.Connect(option);
                redisDB = redisClient.GetDatabase();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
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
