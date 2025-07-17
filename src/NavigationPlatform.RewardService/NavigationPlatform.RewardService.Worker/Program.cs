using NavigationPlatform.RewardService.Worker;
using NavigationPlatform.Shared.Middlewares;
using NavigationPlatform.Shared.Validation;
using Serilog;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();

OverrideSecretsFromEnvironment(builder.Configuration);


builder.Configuration.AddEnvironmentVariables();
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console(new Serilog.Formatting.Json.JsonFormatter())
    .CreateLogger();

builder.Services.AddHttpContextAccessor();


builder.Host.UseSerilog((context, configuration) =>
{
    configuration.ReadFrom.Configuration(context.Configuration)
        .Enrich.WithProperty("ServiceName", "Journey Service");
});

// Add services to the container.
builder.Services.AddApplicationServices(builder.Configuration);

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ValidationMappingMiddleware>();


app.UseAuthorization();


// Health Check endpoints
app.MapHealthChecks("/healthz");
app.MapHealthChecks("/readyz");

 app.Services.RunDatabaseMigrations();

app.Run();



void OverrideSecretsFromEnvironment(IConfiguration configuration)
{
    // Override Database connection
    var rewardsDbConnection = Environment.GetEnvironmentVariable("REWARDS_DB_CONNECTION");
    if (!string.IsNullOrEmpty(rewardsDbConnection))
    {
        configuration["ConnectionStrings:RewardsDb"] = rewardsDbConnection;
    }

    // Override Redis connection
    var redisConnection = Environment.GetEnvironmentVariable("REDIS_CONNECTION");
    if (!string.IsNullOrEmpty(redisConnection))
    {
        configuration["ConnectionStrings:Redis"] = redisConnection;
    }

    // Override Kafka connection
    var kafkaConnection = Environment.GetEnvironmentVariable("KAFKA_CONNECTION");
    if (!string.IsNullOrEmpty(kafkaConnection))
    {
        configuration["Kafka:BootstrapServers"] = kafkaConnection;
    }
}