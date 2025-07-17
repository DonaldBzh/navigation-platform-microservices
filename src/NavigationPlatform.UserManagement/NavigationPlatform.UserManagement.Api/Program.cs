using NavigationPlatform.Shared.Middlewares;
using NavigationPlatform.Shared.Validation;
using NavigationPlatform.UserManagement.Application;
using NavigationPlatform.UserManagement.Infrastructure;
using Serilog;

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

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
//builder.Services.AddAuthorization();

//add health check
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<CorrelationIdMiddleware>();

app.UseMiddleware<ValidationMappingMiddleware>();

app.MapControllers();

await app.Services.RunDatabaseMigrationsAsync();

app.MapHealthChecks("/healthz");
app.MapHealthChecks("/readyz");
app.Run();



void OverrideSecretsFromEnvironment(IConfiguration configuration)
{
    // Override Database connection
    var usersDbConnection = Environment.GetEnvironmentVariable("USERS_DB_CONNECTION");
    if (!string.IsNullOrEmpty(usersDbConnection))
    {
        configuration["ConnectionStrings:UsersDb"] = usersDbConnection;
    }

    // Override Kafka connection
    var kafkaConnection = Environment.GetEnvironmentVariable("KAFKA_CONNECTION");
    if (!string.IsNullOrEmpty(kafkaConnection))
    {
        configuration["Kafka:BootstrapServers"] = kafkaConnection;
    }
}