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

namespace RpgCollector.Middlewares
{
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

            redisClient.Dispose();
            await _next(httpContext);
        }

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
            redisDB = redisClient.GetDatabase();

            try
            {
                if(!await redisDB.KeyExistsAsync(userName))
                {
                    return false;
                }
                
                string redisString = await redisDB.StringGetAsync(userName);
                RedisUser redisUser = JsonSerializer.Deserialize<RedisUser>(redisString);

                if (redisUser.AuthToken != authToken)
                {
                    return false;
                }

                httpContext.Items["User-Id"] = Convert.ToString(redisUser.UserId);

                return true;

            }catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

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
