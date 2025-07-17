using Confluent.Kafka;
using NavigationPlatform.RewardService.Worker.Events;
using System.Text.Json;

namespace NavigationPlatform.RewardService.Worker.Services;

public interface ICacheService
{
    Task<string?> GetAsync(string key);
    Task SetAsync(string key, string value, TimeSpan? expiry = null);
    Task<decimal> GetDailyTotalAsync(Guid userId, DateTime date);
    Task SetDailyTotalAsync(Guid userId, DateTime date, decimal total);
}