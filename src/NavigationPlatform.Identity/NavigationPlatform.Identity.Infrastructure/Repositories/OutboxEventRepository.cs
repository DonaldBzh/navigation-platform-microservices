using NavigationPlatform.Identity.Infrastructure.Persistence;
using NavigationPlatform.Shared.OutBox;

namespace NavigationPlatform.Identity.Infrastructure.Repositories;

public class OutboxEventRepository : IOutboxEventRepository
{
    private readonly IdentityDbContext _dbContext;
    public OutboxEventRepository(IdentityDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task AddAsync(OutboxEvent outboxEvent)
        => await _dbContext.OutboxEvents.AddAsync(outboxEvent);
}