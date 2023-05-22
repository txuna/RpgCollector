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
using RpgCollector.Models.NoticeModel;
using ZLogger;
using RpgCollector.Models.InitPlayerModel;
using RpgCollector.Models.AccountModel;
using CloudStructures;
using CloudStructures.Structures;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using RpgCollector.Models.ChatModel;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

/* 
 ������ �о� ���̰� ���� ��ü�� ���
 */
IConfiguration configuration = builder.Configuration;

builder.Services.Configure<DbConfig>(configuration.GetSection(nameof(DbConfig)));
builder.Services.Configure<ConfigLobby>(configuration.GetSection("LobbySuffix"));
builder.Services.AddTransient<IAccountDB, AccountDB>(); 
builder.Services.AddTransient<INoticeMemoryDB, NoticeMemoryDB>();
builder.Services.AddTransient<IPlayerAccessDB, PlayerAccessDB>();
builder.Services.AddTransient<IMailboxAccessDB, MailboxAccessDB>();
builder.Services.AddTransient<IPackagePaymentDB, PackagePaymentDB>();
builder.Services.AddTransient<IEnchantDB, EnchantDB>();
builder.Services.AddTransient<IAttendanceDB, AttendanceDB>();
builder.Services.AddTransient<IDungeonStageDB, DungeonStageDB>();

builder.Services.AddSingleton<IMasterDataDB, MasterDataDB>();
builder.Services.AddSingleton<IRedisMemoryDB, RedisMemoryDB>();

builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.AllowSynchronousIO = true;
});


SettingLogger();

var app = builder.Build();

var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
LogManager.SetLoggerFactory(loggerFactory, "Global");

app.UseRouting();
// ���� Ȯ�� �̵����
app.UseAuthenticationMiddleware(configuration.GetSection("DbConfig")["RedisDB"]);
//app.MapControllers();
#pragma warning disable ASP0014
app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
#pragma warning restore ASP0014

if (!await LoadData())
{
    return;
}

app.Run();

void SettingLogger()
{
    var logging = builder.Logging;
    logging.ClearProviders();

    // Add Rolling File Logging.
    logging.AddZLoggerRollingFile(
        (dt, x) => $"./logs/access_{dt.ToLocalTime():yyyy-MM-dd}_{x:000}.log",
        x => x.ToLocalTime().Date, 1024,
        options =>
        {
            options.EnableStructuredLogging = true;
            var time = JsonEncodedText.Encode("Timestamp");
            var timeValue = JsonEncodedText.Encode(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));

            options.StructuredLoggingFormatter = (writer, info) =>
            {
                writer.WriteString(time, timeValue);
                info.WriteToJsonWriter(writer);
            };
        });

    logging.AddZLoggerConsole(options =>
    {
        options.EnableStructuredLogging = true;
        var time = JsonEncodedText.Encode("EventTime");
        var timeValue = JsonEncodedText.Encode(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));

        options.StructuredLoggingFormatter = (writer, info) =>
        {
            writer.WriteString(time, timeValue);
            info.WriteToJsonWriter(writer);
        };
    });
}

/*
 * �����ͺ��̽��� �ִ� ���������� Redis������ �ű�� �۾�
 * �����ͺ��̽��� �ִ� ������ �����͸� Redis������ �ű�� �۾�
 */
async Task<bool> LoadData()
{
    string redisDBAddress = configuration.GetSection("DbConfig")["RedisDB"];
    //string password = configuration.GetSection("DbConfig")["RedisPassword"];

    //ConfigurationOptions option = ConfigurationOptions.Parse(redisDBAddress);
    //option.Password = password;

    //var config = new RedisConfig("default", option);
    var config = new RedisConfig("default", redisDBAddress);
    var _redisConn = new RedisConnection(config);

    try
    {
        // Redis�� ���������� �����Ѵ�. + �̹� Notices Ű�� �ִٸ� ����
        if(await LoadNotice(_redisConn) == false)
        {
            return false;
        }

        if(await LoadVersion(_redisConn) == false)
        {
            return false;
        }

    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
        return false;
    }

    return true;
}

async Task<bool> LoadNotice(RedisConnection redisConn)
{
    try
    {
        var redis = new RedisList<Notice>(redisConn, "Notice", null);
        await redis.DeleteAsync();
        await redis.RightPushAsync(new Notice
        {
            NoticeId = 1,
            Title = "[�������]",
            Content = "������ ������ �������� ���Ͽ� ������ ��� �ߴܵ� �����Դϴ�.",
            UploaderId = 1,
        });

        await redis.RightPushAsync(new Notice
        {
            NoticeId = 2,
            Title = "[�⼮���� �̺�Ʈ �ȳ�]",
            Content = "2023�⵵ �ٽ� ���ƿ� �⼮���� �̺�Ʈ�ȳ��Դϴ�.",
            UploaderId = 1,
        });

        return true;
    }
    catch(Exception ex)
    {
        Console.WriteLine(ex.Message);
        return false;
    }
}

async Task<bool> LoadVersion(RedisConnection redisConn)
{
    try
    {
        var gameVersion = new GameVersion { ClientVersion = "1.0.0" , MasterVersion = "1.0.0"};
        var redis = new RedisString<GameVersion>(redisConn, "Version", null);
        if(await redis.SetAsync(gameVersion, null) == false)
        {
            return false;
        }
        return true;
    }
    catch(Exception ex)
    {
        Console.WriteLine(ex.Message);
        return false;
    }
}