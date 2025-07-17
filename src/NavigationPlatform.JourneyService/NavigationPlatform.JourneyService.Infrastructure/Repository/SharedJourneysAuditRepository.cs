using NavigationPlatform.JourneyService.Domain.Entities;
using NavigationPlatform.JourneyService.Domain.Interfaces;
using NavigationPlatform.JourneyService.Infrastructure.Persistance;

namespace NavigationPlatform.JourneyService.Infrastructure.Repository;

public class SharedJourneysAuditRepository : ISharedJourneysAuditRepository
{
    private readonly ApplicationDbContext _dbContext;
    public SharedJourneysAuditRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void AddAudit(SharingAuditLog journey, CancellationToken token = default)
    {
        _dbContext.SharingAuditLogs.AddAsync(journey, token);
    }
}
