using RpgCollector.Middlewares;
using RpgCollector.Models;
using RpgCollector.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
//builder.Services.AddHttpContextAccessor();


/* 
 ������ �о� ���̰� ���� ��ü�� ���
 */
IConfiguration configuration = builder.Configuration;
builder.Services.Configure<DbConfig>(configuration.GetSection(nameof(DbConfig)));
builder.Services.AddTransient<ICustomAuthenticationService, AuthenticationService>();

//builder.Services.AddDistributedMemoryCache( options =>
//{

//});


//builder.Services.AddSession(options =>
//{
//    options.IdleTimeout = TimeSpan.FromSeconds(3000);
//    options.Cookie.HttpOnly = true;
//    options.Cookie.IsEssential = true;
//});

//builder.Services.AddSwaggerGen();

var app = builder.Build();

//app.UseSwagger();
//app.UseSwaggerUI();

app.UseRouting();
//app.UseSession();

// ���� Ȯ��
app.UseAuthenticationMiddleware(configuration.GetSection("DbConfig")["RedisDB"]);

app.MapControllers();

app.Run();
