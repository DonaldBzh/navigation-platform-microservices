using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NavigationPlatform.Gateway.Api;
using Serilog;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();

OverrideSecretsFromEnvironment(builder.Configuration);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console(new Serilog.Formatting.Json.JsonFormatter())
    .CreateLogger();

builder.Host.UseSerilog((context, configuration) =>
{
    configuration.ReadFrom.Configuration(context.Configuration)
        .Enrich.WithProperty("ServiceName", "Yarp-Gateway");
});

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();


// Add YARP
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});


// Configure Swagger to show all microservice endpoints
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Navigation Platform API Gateway",
        Version = "v1",
        Description = "API Gateway for Navigation Platform - Routes to Identity, User Management, and Journey services"
    });

    // Add JWT authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });

    // Add servers for different environments
    c.AddServer(new OpenApiServer
    {
        Url = "https://localhost:5043",
        Description = "Development Gateway"
    });
});

// Add JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Auth:Issuer"],
            ValidAudience = builder.Configuration["Auth:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Auth:Key"]!)),
        };

        // Log authentication events
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Log.Warning("Authentication failed: {Error}", context.Exception.Message);
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                var userId = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
                Log.Information("Token validated for user: {UserId}", userId);
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("RequireAuth", policy => policy.RequireAuthenticatedUser())
    .AddPolicy("RequireAdmin", policy => policy.RequireRole("Admin"))
    .AddPolicy("RequireUser", policy => policy.RequireRole("User"));

// Add Health Checks
builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Navigation Platform API Gateway v1");
        c.RoutePrefix = "swagger";

        // Add links to individual service Swagger UIs for development
        c.ConfigObject.AdditionalItems.Add("urls", new[]
        {
            new { url = "/swagger/v1/swagger.json", name = "Gateway" },
            new { url = "https://localhost:5044/swagger/v1/swagger.json", name = "Identity Service" },
            new { url = "https://localhost:5045/swagger/v1/swagger.json", name = "User Management Service" },
            new { url = "https://localhost:5046/swagger/v1/swagger.json", name = "Journey Service" }
        });
    });
}

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

app.Use(async (context, next) =>
{
    // Handle correlation ID
    const string CorrelationIdHeader = "X-Correlation-ID";
    string correlationId = context.Request.Headers[CorrelationIdHeader].FirstOrDefault();

    if (string.IsNullOrEmpty(correlationId))
    {
        correlationId = Guid.NewGuid().ToString();
        context.Request.Headers[CorrelationIdHeader] = correlationId;
    }

    context.Response.Headers[CorrelationIdHeader] = correlationId;

    // Push correlation ID into Serilog context
    using (Serilog.Context.LogContext.PushProperty("CorrelationId", correlationId))
    {
        // Copy user claims to headers if authenticated
        if (context.User.Identity.IsAuthenticated)
        {
            var userId = context.User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
            var userEmail = context.User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")?.Value;
            var userRole = context.User.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value;

            if (!string.IsNullOrEmpty(userId))
                context.Request.Headers["X-User-Id"] = userId;
            if (!string.IsNullOrEmpty(userEmail))
                context.Request.Headers["X-User-Email"] = userEmail;
            if (!string.IsNullOrEmpty(userRole))
                context.Request.Headers["X-User-Role"] = userRole;

            Console.WriteLine($"DEBUG: Setting headers - UserId: {userId}, Role: {userRole}");
        }

        await next();
    }
});

// Health check endpoints
app.MapHealthChecks("/healthz", new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/readyz");

// Map YARP routes
app.MapReverseProxy();

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

    // Override service URLs
    var identityServiceUrl = Environment.GetEnvironmentVariable("IDENTITY_SERVICE_URL");
    if (!string.IsNullOrEmpty(identityServiceUrl))
    {
        configuration["ReverseProxy:Clusters:identity-cluster:Destinations:identity-service:Address"] = identityServiceUrl;
    }

    var userServiceUrl = Environment.GetEnvironmentVariable("USER_SERVICE_URL");
    if (!string.IsNullOrEmpty(userServiceUrl))
    {
        configuration["ReverseProxy:Clusters:user-cluster:Destinations:user-service:Address"] = userServiceUrl;
    }

    var journeyServiceUrl = Environment.GetEnvironmentVariable("JOURNEY_SERVICE_URL");
    if (!string.IsNullOrEmpty(journeyServiceUrl))
    {
        configuration["ReverseProxy:Clusters:journey-cluster:Destinations:journey-service:Address"] = journeyServiceUrl;
    }
}