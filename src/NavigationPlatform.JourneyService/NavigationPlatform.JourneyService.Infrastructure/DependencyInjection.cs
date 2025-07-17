using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using NavigationPlatform.JourneyService.Application.Interfaces;
using NavigationPlatform.JourneyService.Domain.Interfaces;
using NavigationPlatform.JourneyService.Infrastructure.Configuration;
using NavigationPlatform.JourneyService.Infrastructure.Persistance;
using NavigationPlatform.JourneyService.Infrastructure.Repository;
using NavigationPlatform.JourneyService.Infrastructure.Services;
using NavigationPlatform.JourneyService.Infrastructure.Services.Consumers;
using NavigationPlatform.JourneyService.Infrastructure.Services.Producers;
using NavigationPlatform.Shared.Identity;
using NavigationPlatform.Shared.Kafka;
using NavigationPlatform.Shared.OutBox;
using NavigationPlatform.Shared.Persistance;
using System.Text;

namespace NavigationPlatform.JourneyService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure( this IServiceCollection services, IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("JourneyDb")
           ?? throw new ArgumentNullException(nameof(configuration));

        // Database
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.Configure<KafkaOptions>(configuration.GetSection(KafkaOptions.SectionName));

        // services
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<IJourneyRepository, JourneyRepository>();
        services.AddScoped<IJourneyShareRepository, JourneyShareRepository>();
        services.AddScoped<ISharedJourneysAuditRepository, SharedJourneysAuditRepository>();
        services.AddScoped<IPublicJourneyRepository, PublicJourneyRepository>();

      
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IJourneyService, JourneyResultService>();

        services.AddHostedService<RewardWorkerConsumer>();
        services.AddSingleton<IKafkaClientFactory, KafkaClientFactory>();
        services.AddScoped<IRewardWorkerProcessor, RewardWorkerProcessor>();

        services.AddSingleton<IEventProducer, EventProducer>();


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
