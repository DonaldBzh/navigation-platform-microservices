using MediatR;
using Microsoft.EntityFrameworkCore;
using NavigationPlatform.JourneyService.Domain.Entities;
using NavigationPlatform.JourneyService.Domain.Interfaces;
using NavigationPlatform.JourneyService.Infrastructure.Persistance;
using System.Threading;

namespace NavigationPlatform.JourneyService.Infrastructure.Repository;

public class JourneyShareRepository : IJourneyShareRepository
{
    private readonly ApplicationDbContext _dbContext;
    public JourneyShareRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void AddJourney(JourneyShare journey, CancellationToken token = default)
    {
        _dbContext.SharedJourneys.AddAsync(journey, token);
    }

    public async Task<IEnumerable<Journey>> GetSharedJourneysWithUser(Guid userId, CancellationToken token = default)
    {
        var sharedJourneys = await _dbContext.SharedJourneys
              .Include(js => js.Journey)
              .Where(js => js.SharedWithUserId == userId && js.RevokedAt == null)
              .Select(js => js.Journey)
              .ToListAsync(token);

        return sharedJourneys;
    }
}
