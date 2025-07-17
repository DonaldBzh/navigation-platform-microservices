using NavigationPlatform.JourneyService.Domain.Entities;
using NavigationPlatform.JourneyService.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace NavigationPlatform.JourneyService.Domain.Interfaces;

public interface IJourneyRepository
{
    IQueryable<MonthlyUserDistance> GetMonthlyUserDistances();
    void AddJourney(Journey journey, CancellationToken token = default);
    Task<Journey?> GetByIdAndUserIdAsync(Guid id, Guid userId, CancellationToken token = default);
    void DeleteById(Journey journey, CancellationToken token = default);
    Task<IEnumerable<Journey>> GetJourneysAsync(Guid userId, CancellationToken token = default);
    IQueryable<Journey> GetJourneyAsQuerable();
}
