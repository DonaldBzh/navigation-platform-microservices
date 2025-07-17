using Microsoft.Extensions.Logging;
using NavigationPlatform.Shared.OutBox;
using NavigationPlatform.Shared.Persistance;

namespace NavigationPlatform.UserManagement.Infrastructure.Outbox;

public class OutboxEventPublisher : IOutboxPublisher
{
    private readonly IOutboxEventRepository _outboxEventRepository;
    private readonly ILogger<OutboxEventPublisher> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public OutboxEventPublisher(IOutboxEventRepository outboxEventRepository, ILogger<OutboxEventPublisher> logger, IUnitOfWork unitOfWork)
    {
        _outboxEventRepository = outboxEventRepository;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task PublishAsync<T>(T domainEvent) where T : class
    {
        if (domainEvent == null)
            throw new ArgumentNullException(nameof(domainEvent));

        var outboxEvent = OutboxFactory.Create(domainEvent);

        await _outboxEventRepository.AddAsync(outboxEvent);

        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Domain event {EventType} added to outbox with ID {EventId}",
            outboxEvent.EventType, outboxEvent.Id);
    }
}


