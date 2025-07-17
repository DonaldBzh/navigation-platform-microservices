using NavigationPlatform.JourneyService.Domain.Entities;

namespace NavigationPlatform.JourneyService.Domain.Interfaces;

public interface ISharedJourneysAuditRepository
{
    void AddAudit(SharingAuditLog journey, CancellationToken token = default);
}
