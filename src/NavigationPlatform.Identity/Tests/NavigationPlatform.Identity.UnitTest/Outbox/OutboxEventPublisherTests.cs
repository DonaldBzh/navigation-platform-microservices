using Microsoft.Extensions.Logging;
using NavigationPlatform.Identity.Infrastructure.Outbox;
using NavigationPlatform.Shared.OutBox;
using NavigationPlatform.Shared.Persistance;
using NSubstitute;

namespace NavigationPlatform.Identity.UnitTest.Outbox;

public class OutboxEventPublisherTests
{
    private readonly IOutboxEventRepository _outboxRepo = Substitute.For<IOutboxEventRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ILogger<OutboxEventPublisher> _logger = Substitute.For<ILogger<OutboxEventPublisher>>();

    private readonly OutboxEventPublisher _publisher;

    public OutboxEventPublisherTests()
    {
        _publisher = new OutboxEventPublisher(_outboxRepo, _logger, _unitOfWork);
    }

    [Fact]
    public async Task PublishAsync_Should_AddEventToOutbox_And_SaveChanges()
    {
        // Arrange
        var domainEvent = new TestEvent { Property = "value" };

        // Act
        await _publisher.PublishAsync(domainEvent);

        // Assert
        await _outboxRepo.Received(1).AddAsync(Arg.Is<OutboxEvent>(e => e.EventType == nameof(TestEvent)));
        await _unitOfWork.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task PublishAsync_NullEvent_ShouldThrow()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _publisher.PublishAsync<TestEvent>(null!));
    }
}

public class TestEvent
{
    public string Property { get; set; } = null!;
}