using StackExchange.Redis;

namespace NavigationPlatform.RewardService.Worker.Services;

public class CacheService : ICacheService
{
    private readonly IDatabase _database;
    private readonly IConfiguration _configuration;
    private readonly ILogger<CacheService> _logger;

    public CacheService(IConnectionMultiplexer redis, IConfiguration configuration, ILogger<CacheService> logger)
    {
        _database = redis.GetDatabase();
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<string?> GetAsync(string key)
    {
        try
        {
            return await _database.StringGetAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cache key: {Key}", key);
            return null;
        }
    }

    public async Task SetAsync(string key, string value, TimeSpan? expiry = null)
    {
        try
        {
            await _database.StringSetAsync(key, value, expiry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting cache key: {Key}", key);
        }
    }

    public async Task<decimal> GetDailyTotalAsync(Guid userId, DateTime date)
    {
        var key = GenerateDailyTotalKey(userId, date);
        var cachedValue = await GetAsync(key);

        if (cachedValue != null && decimal.TryParse(cachedValue, out var total))
        {
            _logger.LogDebug("Cache hit for daily total: {Key} = {Total}", key, total);
            return total;
        }

        _logger.LogDebug("Cache miss for daily total: {Key}", key);
        return 0m;
    }

    public async Task SetDailyTotalAsync(Guid userId, DateTime date, decimal total)
    {
        var key = GenerateDailyTotalKey(userId, date);
        var ttlHours = _configuration.GetValue<int>("Cache:DailyTotalsTtlHours", 25);
        var expiry = TimeSpan.FromHours(ttlHours);

        await SetAsync(key, total.ToString("F2"), expiry);
        _logger.LogDebug("Set daily total cache: {Key} = {Total}, TTL: {Hours}h", key, total, ttlHours);
    }

    private static string GenerateDailyTotalKey(Guid userId, DateTime date)
    {
        return $"daily_total:{userId}:{date:yyyy-MM-dd}";
    }
}