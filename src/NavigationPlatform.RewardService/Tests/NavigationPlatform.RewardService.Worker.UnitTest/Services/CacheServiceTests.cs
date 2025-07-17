using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NavigationPlatform.RewardService.Worker.Services;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using StackExchange.Redis;
using System;
using System.Reflection;

namespace NavigationPlatform.RewardService.Worker.UnitTest.Services;

public class CacheServiceTests
{
    private readonly IConnectionMultiplexer _connectionMultiplexer;
    private readonly IDatabase _database;
    private readonly IConfiguration _configuration;
    private readonly ILogger<CacheService> _logger;
    private readonly CacheService _sut;

    public CacheServiceTests()
    {
        _connectionMultiplexer = Substitute.For<IConnectionMultiplexer>();
        _database = Substitute.For<IDatabase>();
        _configuration = Substitute.For<IConfiguration>();
        _logger = Substitute.For<ILogger<CacheService>>();

        _connectionMultiplexer.GetDatabase(Arg.Any<int>(), Arg.Any<object>()).Returns(_database);
        _sut = new CacheService(_connectionMultiplexer, _configuration, _logger);
    }

    [Fact]
    public async Task GetAsync_ReturnsValue_WhenKeyExists()
    {
        var key = "test-key";
        _database.StringGetAsync(key).Returns("test-value");

        var result = await _sut.GetAsync(key);

        result.Should().Be("test-value");
    }

    [Fact]
    public async Task GetAsync_ReturnsNull_WhenKeyDoesNotExist()
    {
        _database.StringGetAsync("missing-key").Returns((string?)null);

        var result = await _sut.GetAsync("missing-key");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAsync_LogsErrorAndReturnsNull_WhenExceptionThrown()
    {
        // Arrange
        var key = "error-key";
        var exception = new RedisException("redis error");

        _database.StringGetAsync(key).Throws(exception);

        // Act
        var result = await _sut.GetAsync(key);

        // Assert
        result.Should().BeNull();

        _logger.Received().Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Error getting cache key")),
            exception,
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task SetAsync_SetsKeyWithoutExpiry()
    {
        await _sut.SetAsync("test-key", "value");

        await _database.Received(1).StringSetAsync("test-key", "value", null);
    }

    [Fact]
    public async Task SetAsync_SetsKeyWithExpiry()
    {
        var expiry = TimeSpan.FromMinutes(30);

        await _sut.SetAsync("key", "value", expiry);

        await _database.Received(1).StringSetAsync("key", "value", expiry);
    }

    [Fact]
    public async Task SetAsync_WhenExceptionOccurs_LogsError()
    {
        // Arrange
        var key = "fail-key";
        var value = "test-value";
        var exception = new RedisException("fail");

        _database.StringSetAsync(key, value, null).Throws(exception);

        // Act
        await _sut.SetAsync(key, value);

        // Assert
        _logger.Received().Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Error setting cache key")),
            exception,
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Theory]
    [InlineData("42.50", 42.50)]
    [InlineData("0.00", 0.00)]
    public async Task GetDailyTotalAsync_ReturnsParsedDecimal_WhenCacheHit(string cacheValue, decimal expected)
    {
        var userId = Guid.NewGuid();
        var date = new DateTime(2025, 7, 1);
        var key = $"daily_total:{userId}:{date:yyyy-MM-dd}";

        _database.StringGetAsync(key).Returns(cacheValue);

        var result = await _sut.GetDailyTotalAsync(userId, date);

        result.Should().Be(expected);
    }

    [Fact]
    public async Task GetDailyTotalAsync_ReturnsZero_WhenCacheValueInvalid()
    {
        var userId = Guid.NewGuid();
        var date = new DateTime(2025, 7, 1);
        var key = $"daily_total:{userId}:{date:yyyy-MM-dd}";

        _database.StringGetAsync(key).Returns("invalid");

        var result = await _sut.GetDailyTotalAsync(userId, date);

        result.Should().Be(0m);
    }

    [Fact]
    public async Task GetDailyTotalAsync_ReturnsZero_WhenExceptionOccurs()
    {
        var userId = Guid.NewGuid();
        var date = DateTime.Today;
        var key = $"daily_total:{userId}:{date:yyyy-MM-dd}";
        var ex = new RedisException("fail");

        _database.StringGetAsync(key).Throws(ex);

        var result = await _sut.GetDailyTotalAsync(userId, date);

        result.Should().Be(0m);
        _logger.Received().Log( LogLevel.Error, Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Error getting cache key")),
            ex,Arg.Any<Func<object, Exception?, string>>());
    }


    [Theory]
    [InlineData("2025-07-01")]
    [InlineData("2024-12-31")]
    public void GenerateDailyTotalKey_Should_FormatCorrectly(string dateStr)
    {
        var userId = Guid.NewGuid();
        var date = DateTime.Parse(dateStr);
        var expectedKey = $"daily_total:{userId}:{date:yyyy-MM-dd}";

        var method = typeof(CacheService).GetMethod("GenerateDailyTotalKey", BindingFlags.NonPublic | BindingFlags.Static)!;
        var actualKey = method.Invoke(null, new object[] { userId, date }) as string;

        actualKey.Should().Be(expectedKey);
    }
}

