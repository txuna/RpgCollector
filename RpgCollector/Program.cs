using Microsoft.Extensions.Options;
using MySqlConnector;
using RpgCollector.Middlewares;
using RpgCollector.Models;
using RpgCollector.Services;
using RpgCollector.Utility;
using SqlKata.Compilers;
using SqlKata.Execution;
using StackExchange.Redis;
using System.Data;
using System.Data.Common;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

/* 
 설정값 읽어 들이고 서비스 객체로 등록
 */
IConfiguration configuration = builder.Configuration;

builder.Services.Configure<DbConfig>(configuration.GetSection(nameof(DbConfig)));
builder.Services.AddTransient<IAccountDB, AccountDB>(); 
builder.Services.AddTransient<IAccountMemoryDB, AccountMemoryDB>();   
builder.Services.AddTransient<INoticeMemoryDB, NoticeMemoryDB>();
builder.Services.AddTransient<IPlayerAccessDB, PlayerAccessDB>();
builder.Services.AddTransient<IMailboxAccessDB, MailboxAccessDB>();

var app = builder.Build();

app.UseRouting();
// 인증 확인 미들웨어
app.UseAuthenticationMiddleware(configuration.GetSection("DbConfig")["RedisDB"]);
app.MapControllers();

bool success = await LoadData();
if (!success)
{
    return;
}

app.Run();


/*
 * 데이터베이스에 있는 공지사항을 Redis서버로 옮기는 작업
 * 데이터베이스에 있는 마스터 데이터를 Redis서버로 옮기는 작업
 */
async Task<bool> LoadData()
{
    string redisDBAddress = configuration.GetSection("DbConfig")["RedisDB"];

    ConnectionMultiplexer? redisClient;
    IDatabase redisDB;

    ConfigurationOptions option = new ConfigurationOptions
    {
        EndPoints = { redisDBAddress }
    };

    try
    {
        redisClient = DatabaseConnector.OpenRedis(redisDBAddress);
        if (redisClient == null)
        {
            throw new Exception("DB Connection Failed");
        }
        redisDB = redisClient.GetDatabase();

        // Redis에 공지사항을 저장한다. + 이미 Notices 키가 있다면 날림
        if (await redisDB.KeyExistsAsync("Notices"))
        {
            await redisDB.KeyDeleteAsync("Notices");
        }
        await redisDB.ListRightPushAsync("Notices", JsonSerializer.Serialize(new Notice
        {
            NoticeId = 1,
            Content = "Hello World", 
            UploaderId = 1,
        }));
        await redisDB.ListRightPushAsync("Notices", JsonSerializer.Serialize(new Notice
        {
            NoticeId = 1,
            Content = "Hello World",
            UploaderId = 1,
        }));

        // Client Version과 MasterData Version을 Redis에 등록 
        await redisDB.StringSetAsync("ClientVersion", "1.0.0");
        await redisDB.StringSetAsync("MasterDataVersion", "1.0.0");
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
        return false;
    }
    redisClient.CloseRedis();

    return true;
}