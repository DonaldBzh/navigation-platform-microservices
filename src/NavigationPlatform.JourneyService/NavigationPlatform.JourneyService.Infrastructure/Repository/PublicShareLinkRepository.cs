using Microsoft.EntityFrameworkCore;
using NavigationPlatform.JourneyService.Domain.Entities;
using NavigationPlatform.JourneyService.Domain.Interfaces;
using NavigationPlatform.JourneyService.Infrastructure.Persistance;

namespace NavigationPlatform.JourneyService.Infrastructure.Repository;

public class PublicJourneyRepository : IPublicJourneyRepository
{
    private readonly ApplicationDbContext _dbContext;
    public PublicJourneyRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void AddPublicJourney(PublicJourney journey, CancellationToken token = default)
    {
        _dbContext.PublicJourneys.AddAsync(journey, token);
    }

    public async Task<PublicJourney?> GetPublicJourney(Guid journeyId, Guid userId, CancellationToken token = default)
    {
        var journey = await _dbContext.PublicJourneys
           .FirstOrDefaultAsync(p =>
               p.JourneyId == journeyId && p.CreatedByUserId == userId 
               &&  !p.IsRevoked,token);

        return journey;
    }

    public async Task<PublicJourney?> GetPublicJourneyByToken(string publicToken,CancellationToken token = default)
    {
        var journey = await _dbContext.PublicJourneys
            .Include(j => j.Journey)
           .FirstOrDefaultAsync(p =>
               p.PublicToken == publicToken , token)
           ;

        return journey;
    }
}
