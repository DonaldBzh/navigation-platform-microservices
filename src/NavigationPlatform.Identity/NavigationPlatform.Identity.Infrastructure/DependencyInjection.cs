using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using NavigationPlatform.Identity.Application.Abstractions.Authentication;
using NavigationPlatform.Identity.Domain.Interfaces;
using NavigationPlatform.Identity.Infrastructure.Outbox;
using NavigationPlatform.Identity.Infrastructure.Persistence;
using NavigationPlatform.Identity.Infrastructure.Repositories;
using NavigationPlatform.Identity.Infrastructure.Security;
using NavigationPlatform.Identity.Infrastructure.Services.Consumers;
using NavigationPlatform.Identity.Infrastructure.Services.Kafka;
using NavigationPlatform.Shared.Identity;
using NavigationPlatform.Shared.Kafka;
using NavigationPlatform.Shared.OutBox;
using NavigationPlatform.Shared.Persistance;
using System.Text;


namespace NavigationPlatform.Identity.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
       this IServiceCollection services,
       IConfiguration configuration)
    {
        var serviceName = "NavigationPlatform.Account";
        services.AddPersistence(configuration)
            .AddAuthenticationAndAuthorization(configuration);

        return services;
    }

    private static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        //Db
        string connectionString = configuration.GetConnectionString("IdentityDb") 
            ?? throw new ArgumentNullException(nameof(configuration));

        services.AddDbContext<IdentityDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<IdentityDbContext>());
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IOutboxEventRepository, OutboxEventRepository>();

        var jwtSettingsSection = configuration.GetSection("Auth");
        var jwtSettings = jwtSettingsSection.Get<JwtSettings>();
        services.AddSingleton(jwtSettings);

        services.AddScoped<ITokenRepository, TokenRepository>();
        services.AddScoped<ITokenGenerator, TokenGenerator>();
        services.AddScoped<IPasswordHasher, Sha256PasswordHasher>();

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        services.AddScoped<IOutboxPublisher, OutboxEventPublisher>();

        // Kafka
        services.Configure<KafkaOptions>(configuration.GetSection(KafkaOptions.SectionName));
        services.AddSingleton<IEventProducer, EventProducer>();
        services.AddHostedService<OutboxEventProcessor>();

        services.AddHostedService<UserStatusChangedConsumer>();
        services.AddSingleton<IKafkaClientFactory, KafkaClientFactory>();
        services.AddScoped<IUserStatusChangedProcessor, UserStatusChangedProcessor>();

        return services;
    }

    private static IServiceCollection AddAuthenticationAndAuthorization(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(o =>
        {
            o.TokenValidationParameters = new TokenValidationParameters
            {
                ValidIssuer = configuration["Auth:Issuer"],
                ValidAudience = configuration["Auth:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey
                    (Encoding.UTF8.GetBytes(configuration["Auth:Key"])),
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true
            };
        });


        services.AddAuthorization();

        return services;
    }

    public static async Task RunDatabaseMigrationsAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        ArgumentNullException.ThrowIfNull(dbContext, nameof(dbContext));

        var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();

        if (pendingMigrations.Any())
        {
            await dbContext.Database.MigrateAsync();
        }

    }
}
