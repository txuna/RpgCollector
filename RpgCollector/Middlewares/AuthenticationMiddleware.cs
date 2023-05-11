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
using RpgCollector.Models.AccountModel;
using System.Runtime.InteropServices;
using CloudStructures;
using CloudStructures.Structures;

namespace RpgCollector.Middlewares
{
    public class AuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string[] exclusivePaths = new[] { "/Login", "/Register" };
        private string _redisAddress;
        private RedisConnection _redisConn; 

        public AuthenticationMiddleware(RequestDelegate next, string redisAddress)
        {
            
            _next = next;
            var config = new RedisConfig("default", redisAddress);
            _redisConn = new RedisConnection(config);
        }

        public async Task Invoke(HttpContext httpContext)
        {
            
            if (!await VerifyVersion(httpContext))
            {
                httpContext.Response.StatusCode = 401;
                await httpContext.Response.WriteAsync("Invalid Data Version");
                return;
            }

            if (!CheckExclusivePath(httpContext))
            {
                if (!VerifyHeader(httpContext))
                {
                    httpContext.Response.StatusCode = 401;
                    await httpContext.Response.WriteAsync("Invalid Header");
                    return;
                }

                if (!await VerifyToken(httpContext))
                {
                    httpContext.Response.StatusCode = 401;
                    await httpContext.Response.WriteAsync("Invalid Token");
                    return;
                }
            }

            await _next(httpContext);
        }

        bool CheckExclusivePath(HttpContext httpContext)
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

        bool VerifyHeader(HttpContext httpContext)
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
        async Task<bool> VerifyToken(HttpContext httpContext)
        {
            string? authToken = httpContext.Request.Headers["Auth-Token"];
            string? userName = httpContext.Request.Headers["User-Name"];

            try
            {
                var redis = new RedisString<RedisUser>(_redisConn, userName, null);
                var user = await redis.GetAsync();

                if (!user.HasValue)
                {
                    return false;
                }

                if (user.Value.AuthToken != authToken)
                {
                    return false;
                }

                SetUserIdInHttpContext(httpContext, user.Value.UserId);

                return true;

            }catch (Exception ex)
            {
                return false;
            }
        }

        void SetUserIdInHttpContext(HttpContext httpContext, int userId)
        {
            httpContext.Items["User-Id"] = Convert.ToString(userId);
        }

        async Task<bool> VerifyVersion(HttpContext httpContext)
        {
            string? requestClientVersion = httpContext.Request.Headers["Client-Version"];
            string? requestMasterDataVersion = httpContext.Request.Headers["MasterData-Version"];

            var redis = new RedisString<GameVersion>(_redisConn, "Version", null);
            var gameVersion = await redis.GetAsync();

            if (!gameVersion.HasValue)
            {
                return false;
            }

            string? redisClientVersion = gameVersion.Value.ClientVersion;
            string? redisMasterDataVersion = gameVersion.Value.MasterDataVersion;

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
