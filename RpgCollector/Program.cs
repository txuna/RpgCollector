using Microsoft.Extensions.Options;
using MySqlConnector;
using RpgCollector;
using RpgCollector.Middlewares;
using RpgCollector.Models;
using RpgCollector.Services;
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
 ������ �о� ���̰� ���� ��ü�� ���
 */
IConfiguration configuration = builder.Configuration;
builder.Services.Configure<DbConfig>(configuration.GetSection(nameof(DbConfig)));
builder.Services.AddTransient<ICustomAuthenticationService, AuthenticationService>();
builder.Services.AddTransient<INoticeService, NoticeService>();

var app = builder.Build();

app.UseRouting();
// ���� Ȯ�� �̵����
app.UseAuthenticationMiddleware(configuration.GetSection("DbConfig")["RedisDB"]);
app.MapControllers();

bool success = await LoadData();
if (!success)
{
    return;
}

app.Run();


/*
 * �����ͺ��̽��� �ִ� ���������� Redis������ �ű�� �۾�
 * �����ͺ��̽��� �ִ� ������ �����͸� Redis������ �ű�� �۾�
 */
async Task<bool> LoadData()
{
    string redisDBAddress = configuration.GetSection("DbConfig")["RedisDB"];
    string gameDBAddress = configuration.GetSection("DbConfig")["MysqlGameDB"];

    QueryFactory _gameQueryFactory;
    IDbConnection? _gameDbConnection;
    MySqlCompiler _gameCompiler;
    ConnectionMultiplexer? redisClient;
    IDatabase redisDB;

    ConfigurationOptions option = new ConfigurationOptions
    {
        EndPoints = { redisDBAddress }
    };

    try
    {
        redisClient = DatabaseSuppoter.OpenRedis(redisDBAddress);
        _gameDbConnection = DatabaseSuppoter.OpenMysql(gameDBAddress);
        if(redisClient == null || _gameDbConnection == null)
        {
            throw new Exception("DB Connection Failed");
        }
        redisDB = redisClient.GetDatabase();

        _gameCompiler = new MySqlCompiler();
        _gameQueryFactory = new QueryFactory(_gameDbConnection, _gameCompiler);

        // �����ͺ��̽��� �����ϴ� ���������� ������´�. 
        IEnumerable<Notice> notices = await _gameQueryFactory.Query("notices").GetAsync<Notice>();
        // Redis�� ���������� �����Ѵ�. + �̹� Notices Ű�� �ִٸ� ����
        if (await redisDB.KeyExistsAsync("Notices"))
        {
            await redisDB.KeyDeleteAsync("Notices");
        }
        foreach(Notice value in notices)
        {
            await redisDB.ListRightPushAsync("Notices", JsonSerializer.Serialize(value)); 
        }
    }
    catch (Exception e)
    {
        return false;
    }

    _gameDbConnection.CloseMysql();
    redisClient.CloseRedis();



    return true;
}