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
using Newtonsoft.Json;
using RpgCollector.RequestResponseModel;

namespace RpgCollector.Middlewares;

public class AuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string[] exclusivePaths = new[] { "/Login", "/Register" };
    private readonly Services.IRedisMemoryDB _memoryDB;

    string userName;
    string authToken;
    string clientVersion;
    string masterVersion;

    public AuthenticationMiddleware(RequestDelegate next, string redisAddress, Services.IRedisMemoryDB memoryDB)
    {
        
        _next = next;
        _memoryDB = memoryDB;
    }

    public async Task Invoke(HttpContext httpContext)
    {

        httpContext.Request.EnableBuffering();

        using(var reader = new StreamReader(httpContext.Request.Body, Encoding.UTF8, true, 4096, true))
        {
            var bodyStr = await reader.ReadToEndAsync();

            if(await IsNullBodyDataThenSendError(httpContext, bodyStr))
            {
                return;
            }

            var document = JsonDocument.Parse(bodyStr);
            if (IsInvalidJsonFormatThenSendError(httpContext, 
                                                document, 
                                                out userName, 
                                                out authToken, 
                                                out clientVersion, 
                                                out masterVersion))
            {
                return;
            }

            GameVersion? gameVersion = await _memoryDB.GetGameVersion();
            if(gameVersion == null)
            {
                return;
            }

            if (VerifyVersion(gameVersion) == false)
            {
                return;
            }


            if (!CheckExclusivePath(httpContext))
            {
                RedisUser? user = await _memoryDB.GetUser(userName);
                if (user == null)
                {
                    return;
                }

                if (VerifyAccount(user) == false)
                {
                    return;
                }

                SetUserIdInHttpContext(httpContext, user.UserId);
                SetAuthTokenInHttpContext(httpContext, user.AuthToken);
            }
        }

        httpContext.Request.Body.Position = 0;

        await _next(httpContext);
    }

    private bool VerifyVersion(GameVersion gameVersion)
    {
        if(gameVersion.ClientVersion != clientVersion)
        {
            return false;
        }
        if(gameVersion.MasterVersion != masterVersion)
        {
            return false;
        }
        return true;
    }

    private bool VerifyAccount(RedisUser _redisUser)
    {
        if(_redisUser.AuthToken != authToken)
        {
            return false;
        }
        return true;
    }

    private bool CheckExclusivePath(HttpContext httpContext)
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

    void SetUserIdInHttpContext(HttpContext httpContext, int userId)
    {
        httpContext.Items["User-Id"] = Convert.ToString(userId);
    }

    void SetAuthTokenInHttpContext(HttpContext httpContext, string authToken)
    {
        httpContext.Items["Auth-Token"] = authToken;
    }

    async Task<bool> IsNullBodyDataThenSendError(HttpContext context, string bodyStr)
    {
        if (string.IsNullOrEmpty(bodyStr) == false)
        {
            return false;
        }

        var errorJsonResponse = JsonConvert.SerializeObject(new MiddlewareResponse
        {
            result = ErrorCode.InValidRequestHttpBody
        });
        var bytes = Encoding.UTF8.GetBytes(errorJsonResponse);
        await context.Response.Body.WriteAsync(bytes, 0, bytes.Length);

        return true;
    }

    bool IsInvalidJsonFormatThenSendError(HttpContext context, 
                                          JsonDocument document, 
                                          out string userName,
                                          out string authToken,
                                          out string clientVersion, 
                                          out string masterVersion)
    {
        try
        {
            userName = document.RootElement.GetProperty("UserName").GetString();
            authToken = document.RootElement.GetProperty("AuthToken").GetString();
            clientVersion = document.RootElement.GetProperty("ClientVersion").GetString();
            masterVersion = document.RootElement.GetProperty("MasterVersion").GetString();

            return false;
        }
        catch
        {
            userName = ""; authToken = ""; clientVersion = ""; masterVersion = "";

            var errorJsonResponse = JsonConvert.SerializeObject(new MiddlewareResponse
            {
                result = ErrorCode.AuthTokenFailWrongKeyword
            });

            var bytes = Encoding.UTF8.GetBytes(errorJsonResponse);
            context.Response.Body.Write(bytes, 0, bytes.Length);

            return true;
        }
    }
}
public class MiddlewareResponse
{
    public ErrorCode result { get; set; }
}

// Extension method used to add the middleware to the HTTP request pipeline.
public static class AuthenticationMiddlewareExtensions
{
    public static IApplicationBuilder UseAuthenticationMiddleware(this IApplicationBuilder builder, string redisAddress)
    {
        return builder.UseMiddleware<AuthenticationMiddleware>(redisAddress);
    }
}
