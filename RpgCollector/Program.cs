using RpgCollector.Middlewares;
using RpgCollector.Models;
using RpgCollector.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

/* 
 설정값 읽어 들이고 서비스 객체로 등록
 */
IConfiguration configuration = builder.Configuration;
builder.Services.Configure<DbConfig>(configuration.GetSection(nameof(DbConfig)));
builder.Services.AddTransient<ICustomAuthenticationService, AuthenticationService>();

var app = builder.Build();

app.UseRouting();
// 인증 확인 미들웨어
app.UseAuthenticationMiddleware(configuration.GetSection("DbConfig")["RedisDB"]);
app.MapControllers();

bool success = await InitGame();
if (!success)
{
    return;
}

app.Run();


/*
  초기 설정 함수 
 */
async Task<bool> InitGame()
{
    string redisDBAddress = configuration.GetSection("DbConfig")["RedisDB"];
    string accountDBAddress = configuration.GetSection("DbConfig")["MysqlAccountDB"];
    string gameDBAddress = configuration.GetSection("DbConfig")["MysqlGameDB"];
    // Database 공지사항 -> Redis 

    // Database 마스터데이터 -> Redis 

    // Database 출석부아이템 -> Redis 

    return true;
}