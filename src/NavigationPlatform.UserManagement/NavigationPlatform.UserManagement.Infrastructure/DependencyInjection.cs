using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NavigationPlatform.Shared.Identity;
using NavigationPlatform.Shared.Kafka;
using NavigationPlatform.Shared.OutBox;
using NavigationPlatform.Shared.Persistance;
using NavigationPlatform.UserManagement.Domain.Interfaces;
using NavigationPlatform.UserManagement.Infrastructure.Messaging.Consumers;
using NavigationPlatform.UserManagement.Infrastructure.Messaging.Kafka;
using NavigationPlatform.UserManagement.Infrastructure.Outbox;
using NavigationPlatform.UserManagement.Infrastructure.Persistance;
using NavigationPlatform.UserManagement.Infrastructure.Repositories;

namespace NavigationPlatform.UserManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("UsersDb")
           ?? throw new ArgumentNullException(nameof(configuration));

        // Database
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));

        //domain services
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserAuditRepository, UserAuditRepository>();
        services.AddScoped<IOutboxEventRepository, OutboxEventRepository>();

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        services.AddScoped<IOutboxPublisher, OutboxEventPublisher>();
        // Kafka
        services.Configure<KafkaSettings>(configuration.GetSection("Kafka"));
        services.AddSingleton<IEventProducer, KafkaProducer>();
        services.AddHostedService<OutboxEventProcessor>();
        services.AddHostedService<UserCreatedEventConsumer>();
        return services;
    }

    public static async Task RunDatabaseMigrationsAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        ArgumentNullException.ThrowIfNull(dbContext, nameof(dbContext));

        var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();

        if (pendingMigrations.Any())
        {
            await dbContext.Database.MigrateAsync();
        }


    }
}
