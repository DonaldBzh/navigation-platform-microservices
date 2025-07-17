using Microsoft.EntityFrameworkCore;
using NavigationPlatform.RewardService.Worker.Configuration;
using NavigationPlatform.RewardService.Worker.Persistence;
using NavigationPlatform.RewardService.Worker.Repositories;
using NavigationPlatform.RewardService.Worker.Services;
using NavigationPlatform.Shared.MediatR;
using NavigationPlatform.Shared.Persistance;
using StackExchange.Redis;

namespace NavigationPlatform.RewardService.Worker;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        //Db Context
        string connectionString = configuration.GetConnectionString("RewardsDb")
           ?? throw new ArgumentNullException(nameof(configuration));

        services.AddDbContext<RewardDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<RewardDbContext>());

        //Redis 
        services.AddSingleton<IConnectionMultiplexer>(provider =>
        {
            var connectionString = configuration.GetConnectionString("Redis");
            var redisConfiguration = ConfigurationOptions.Parse(connectionString!);

            return ConnectionMultiplexer.Connect(redisConfiguration);
        });

        // Repositories
        services.AddScoped<IDailyGoalAchievementRepository, DailyGoalAchievementRepository>();


        // Configuration Options
        services.Configure<KafkaOptions>(configuration.GetSection(KafkaOptions.SectionName));
        services.Configure<BusinessRulesOptions>(configuration.GetSection(BusinessRulesOptions.SectionName));

        // Services
        services.AddScoped<ICacheService, CacheService>();
        services.AddSingleton<IEventProducer, EventProducer>();

        // MediatR
        services.AddMediatRServices<IAssemblyMarker>();

        // Kafka Consumer as Hosted Service
        services.AddHostedService<JourneyConsumerService>();

        // Health Checks
        services.AddHealthChecks()
            .AddNpgSql(configuration.GetConnectionString("RewardsDb")!)
            .AddRedis(configuration.GetConnectionString("Redis")!)
            .AddCheck("kafka", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("Kafka consumer running"));

        return services;
    }
    public static void RunDatabaseMigrations(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<RewardDbContext>();
        ArgumentNullException.ThrowIfNull(dbContext, nameof(dbContext));

        var pendingMigrations = dbContext.Database.GetPendingMigrations();

        if (pendingMigrations.Any())
        {
            dbContext.Database.Migrate();
        }

    }

}
public interface IAssemblyMarker { }
