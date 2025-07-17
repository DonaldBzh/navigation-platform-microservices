using Microsoft.EntityFrameworkCore;
using NavigationPlatform.JourneyService.Domain.Entities;
using NavigationPlatform.JourneyService.Domain.Interfaces;
using NavigationPlatform.JourneyService.Domain.ValueObjects;
using NavigationPlatform.JourneyService.Infrastructure.Persistance;

namespace NavigationPlatform.JourneyService.Infrastructure.Repository;

public class JourneyRepository : IJourneyRepository
{
    private readonly ApplicationDbContext _dbContext;
    public JourneyRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void AddJourney(Journey journey, CancellationToken token = default)
    {
        _dbContext.AddAsync(journey, token);
    }

    public void DeleteById(Journey journey, CancellationToken token = default)
    {
        journey.IsDeleted = true;
    }

    public async Task<Journey?> GetByIdAndUserIdAsync(Guid id, Guid userId, CancellationToken token = default)
        => await _dbContext.Journeys
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId, token);

    public IQueryable<Journey> GetJourneyAsQuerable()
        => _dbContext.Journeys.AsQueryable();


    public async Task<IEnumerable<Journey>> GetJourneysAsync(Guid userId, CancellationToken token = default)
    {
        var result = await _dbContext.Journeys
            .Where(j => j.UserId == userId && j.IsDeleted == false)
            .ToListAsync();

        return result;
    }

    public IQueryable<MonthlyUserDistance> GetMonthlyUserDistances()
    {
        return from j in _dbContext.Journeys
               where !j.IsDeleted
               group j by new
               {
                   j.UserId,
                   Year = j.StartDate.Year,
                   Month = j.StartDate.Month
               }
              into g
               select new MonthlyUserDistance
               {
                   UserId = (Guid)g.Key.UserId,
                   Year = g.Key.Year,
                   Month = g.Key.Month,
                   TotalDistanceKm = g.Sum(x => x.RouteDistanceKm)
               };
    }
}
