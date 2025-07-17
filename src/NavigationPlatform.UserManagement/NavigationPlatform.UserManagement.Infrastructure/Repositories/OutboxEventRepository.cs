using NavigationPlatform.Shared.OutBox;
using NavigationPlatform.UserManagement.Domain.Entities;
using NavigationPlatform.UserManagement.Domain.Interfaces;
using NavigationPlatform.UserManagement.Infrastructure.Persistance;

namespace NavigationPlatform.UserManagement.Infrastructure.Repositories;

public class OutboxEventRepository : IOutboxEventRepository
{
    private readonly ApplicationDbContext _dbContext;
    public OutboxEventRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task AddAsync(OutboxEvent outboxEvent)
        => await _dbContext.OutboxEvents.AddAsync(outboxEvent);
}