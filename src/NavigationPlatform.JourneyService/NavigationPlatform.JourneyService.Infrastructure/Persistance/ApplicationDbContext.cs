using Microsoft.EntityFrameworkCore;
using NavigationPlatform.JourneyService.Domain.Entities;
using NavigationPlatform.Shared.Persistance;

namespace NavigationPlatform.JourneyService.Infrastructure.Persistance;

public class ApplicationDbContext : DbContext, IUnitOfWork
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Journey> Journeys => Set<Journey>();
    public DbSet<JourneyShare> SharedJourneys => Set<JourneyShare>();
    public DbSet<PublicJourney> PublicJourneys=> Set<PublicJourney>();
    public DbSet<SharingAuditLog> SharingAuditLogs => Set<SharingAuditLog>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            int result = await base.SaveChangesAsync(cancellationToken);

            return result;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new ApplicationException("Concurrency exception occurred.", ex);
        }
    }
}
