using NavigationPlatform.JourneyService.Api;
using NavigationPlatform.JourneyService.Application;
using NavigationPlatform.JourneyService.Infrastructure;
using NavigationPlatform.Shared.Middlewares;
using NavigationPlatform.Shared.Validation;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();

OverrideSecretsFromEnvironment(builder.Configuration);

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

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services
    .AddInfrastructure(builder.Configuration)
    .AddApplication()
    .AddSwaggerConfiguration();

builder.Services.AddHealthChecks();

builder.Services.AddSwaggerGen();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseMiddleware<CorrelationIdMiddleware>();

app.UseAuthorization();


try
{
    await app.Services.RunDatabaseMigrationsAsync();
}
catch (Npgsql.PostgresException ex) when (ex.SqlState == "42P07")
{
    Log.Warning("Database tables already exist, skipping migrations");
}

app.MapControllers();
app.UseMiddleware<ValidationMappingMiddleware>();

app.MapHealthChecks("/healthz");
app.MapHealthChecks("/readyz");

app.Run();


void OverrideSecretsFromEnvironment(IConfiguration configuration)
{
    // Override Database connection
    var journeyDbConnection = Environment.GetEnvironmentVariable("JOURNEYS_DB_CONNECTION");
    if (!string.IsNullOrEmpty(journeyDbConnection))
    {
        configuration["ConnectionStrings:JourneyDb"] = journeyDbConnection;
    }

    // Override Kafka connection
    var kafkaConnection = Environment.GetEnvironmentVariable("KAFKA_CONNECTION");
    if (!string.IsNullOrEmpty(kafkaConnection))
    {
        configuration["Kafka:BootstrapServers"] = kafkaConnection;
    }
}