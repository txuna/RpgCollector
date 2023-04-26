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
 설정값 읽어 들이고 서비스 객체로 등록
 */
IConfiguration configuration = builder.Configuration;
builder.Services.Configure<DbConfig>(configuration.GetSection(nameof(DbConfig)));
builder.Services.AddTransient<ICustomAuthenticationService, AuthenticationService>();
builder.Services.AddTransient<INoticeService, NoticeService>();

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

        // 데이터베이스에 존재하는 공지사항을 가지고온다. 
        IEnumerable<Notice> notices = await _gameQueryFactory.Query("notices").GetAsync<Notice>();
        // Redis에 공지사항을 저장한다. + 이미 Notices 키가 있다면 날림
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