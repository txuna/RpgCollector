using RpgCollector.Middlewares;
using RpgCollector.Models;
using RpgCollector.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

/* 
 ������ �о� ���̰� ���� ��ü�� ���
 */
IConfiguration configuration = builder.Configuration;
builder.Services.Configure<DbConfig>(configuration.GetSection(nameof(DbConfig)));
builder.Services.AddTransient<ICustomAuthenticationService, AuthenticationService>();

var app = builder.Build();

app.UseRouting();
// ���� Ȯ�� �̵����
app.UseAuthenticationMiddleware(configuration.GetSection("DbConfig")["RedisDB"]);
app.MapControllers();

bool success = await InitGame();
if (!success)
{
    return;
}

app.Run();


/*
  �ʱ� ���� �Լ� 
 */
async Task<bool> InitGame()
{
    string redisDBAddress = configuration.GetSection("DbConfig")["RedisDB"];
    string accountDBAddress = configuration.GetSection("DbConfig")["MysqlAccountDB"];
    string gameDBAddress = configuration.GetSection("DbConfig")["MysqlGameDB"];
    // Database �������� -> Redis 

    // Database �����͵����� -> Redis 

    // Database �⼮�ξ����� -> Redis 

    return true;
}