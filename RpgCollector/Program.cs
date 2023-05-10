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

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

/* 
 ������ �о� ���̰� ���� ��ü�� ���
 */
IConfiguration configuration = builder.Configuration;

builder.Services.Configure<DbConfig>(configuration.GetSection(nameof(DbConfig)));
builder.Services.AddTransient<IAccountDB, AccountDB>(); 
builder.Services.AddTransient<IAccountMemoryDB, AccountMemoryDB>();   
builder.Services.AddTransient<INoticeMemoryDB, NoticeMemoryDB>();
builder.Services.AddTransient<IPlayerAccessDB, PlayerAccessDB>();
builder.Services.AddTransient<IMailboxAccessDB, MailboxAccessDB>();
builder.Services.AddTransient<IPackagePaymentDB, PackagePaymentDB>();
builder.Services.AddTransient<IEnchantDB, EnchantDB>();
builder.Services.AddTransient<IAttendanceDB, AttendanceDB>();
builder.Services.AddSingleton<IMasterDataDB, MasterDataDB>();

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

    ConnectionMultiplexer? redisClient;
    IDatabase redisDB;

    ConfigurationOptions option = new ConfigurationOptions
    {
        EndPoints = { redisDBAddress }
    };

    try
    {
        redisClient = ConnectionMultiplexer.Connect(option);
        redisDB = redisClient.GetDatabase();

        // Redis�� ���������� �����Ѵ�. + �̹� Notices Ű�� �ִٸ� ����
        if (await redisDB.KeyExistsAsync("Notices"))
        {
            await redisDB.KeyDeleteAsync("Notices");
        }
        await redisDB.ListRightPushAsync("Notices", JsonSerializer.Serialize(new Notice
        {
            NoticeId = 1,
            Title = "[�������]",
            Content = "������ ������ �������� ���Ͽ� ������ ��� �ߴܵ� �����Դϴ�.", 
            UploaderId = 1,
        }));
        await redisDB.ListRightPushAsync("Notices", JsonSerializer.Serialize(new Notice
        {
            NoticeId = 2,
            Title = "[�⼮���� �̺�Ʈ �ȳ�]",
            Content = "2023�⵵ �ٽ� ���ƿ� �⼮���� �̺�Ʈ�ȳ��Դϴ�.",
            UploaderId = 1,
        }));

        await redisDB.StringSetAsync("Version", JsonSerializer.Serialize(new GameVersion
        {
            ClientVersion = "1.0.0",
            MasterDataVersion = "1.0.0",
        }));

    }
    catch (Exception ex)
    {
        Console.WriteLine("EE");
        Console.WriteLine(ex.Message);
        return false;
    }
    redisClient.Dispose();

    return true;
}