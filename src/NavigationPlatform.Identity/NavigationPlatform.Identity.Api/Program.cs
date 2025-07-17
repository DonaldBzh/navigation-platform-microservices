using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using NavigationPlatform.Identity.Api.Configurations;
using NavigationPlatform.Identity.Application;
using NavigationPlatform.Identity.Infrastructure;
using NavigationPlatform.Shared.Middlewares;
using NavigationPlatform.Shared.Validation;
using Serilog;
using Serilog.Context;
using System.Threading.RateLimiting;

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
        .Enrich.WithProperty("ServiceName", "Identity Service");
});

builder.Services.AddEndpointsApiExplorer();

builder.Services
    .AddInfrastructure(builder.Configuration)
    .AddApplication()
    .AddSwaggerConfiguration();

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("LoginRateLimit", limiterOptions =>
    {
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.PermitLimit = 5;
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 0; 
    });
});


builder.Services.AddHealthChecks();

builder.Services.AddControllers();
var app = builder.Build();

app.UseAuthentication();

app.UseRateLimiter();

app.UseAuthorization();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<CorrelationIdMiddleware>();

app.UseMiddleware<ValidationMappingMiddleware>();

app.MapControllers();


app.MapHealthChecks("/healthz", new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/readyz");
await app.Services.RunDatabaseMigrationsAsync();
app.Run();



void OverrideSecretsFromEnvironment(IConfiguration configuration)
{
    // Override Auth settings
    var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET");
    if (!string.IsNullOrEmpty(jwtSecret))
    {
        configuration["Auth:Key"] = jwtSecret;
    }

    var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER");
    if (!string.IsNullOrEmpty(jwtIssuer))
    {
        configuration["Auth:Issuer"] = jwtIssuer;
    }

    var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE");
    if (!string.IsNullOrEmpty(jwtAudience))
    {
        configuration["Auth:Audience"] = jwtAudience;
    }

    // Override Auth expiry settings
    var accessTokenExpiry = Environment.GetEnvironmentVariable("AUTH_ACCESS_TOKEN_EXPIRY_MINUTES");
    if (!string.IsNullOrEmpty(accessTokenExpiry))
    {
        configuration["Auth:AccessTokenExpiryMinutes"] = accessTokenExpiry;
    }

    var refreshTokenExpiry = Environment.GetEnvironmentVariable("AUTH_REFRESH_TOKEN_EXPIRY_MINUTES");
    if (!string.IsNullOrEmpty(refreshTokenExpiry))
    {
        configuration["Auth:RefreshTokenExpiryMinutes"] = refreshTokenExpiry;
    }

    // Override Database connection
    var identityDbConnection = Environment.GetEnvironmentVariable("IDENTITY_DB_CONNECTION");
    if (!string.IsNullOrEmpty(identityDbConnection))
    {
        configuration["ConnectionStrings:IdentityDb"] = identityDbConnection;
    }

    // Override Kafka connection
    var kafkaConnection = Environment.GetEnvironmentVariable("KAFKA_CONNECTION");
    if (!string.IsNullOrEmpty(kafkaConnection))
    {
        configuration["Kafka:BootstrapServers"] = kafkaConnection;
    }
}