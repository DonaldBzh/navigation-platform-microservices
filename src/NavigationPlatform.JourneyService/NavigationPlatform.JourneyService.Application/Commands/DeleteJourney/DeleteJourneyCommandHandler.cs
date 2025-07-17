using MediatR;
using NavigationPlatform.JourneyService.Domain.Interfaces;
using NavigationPlatform.Shared.Identity;
using NavigationPlatform.Shared.Persistance;

namespace NavigationPlatform.JourneyService.Application.Commands.DeleteJourney;

public class DeleteJourneyCommandHandler : IRequestHandler<DeleteJourneyCommand>
{
    private readonly IJourneyRepository _journeyRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteJourneyCommandHandler(
        IJourneyRepository journeyRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _journeyRepository = journeyRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteJourneyCommand request, CancellationToken cancellationToken)
    {
        var journey = await _journeyRepository.GetByIdAndUserIdAsync(
            request.Id, _currentUserService.GetUserId(), cancellationToken);

        if (journey == null)
            return;

        if (journey.IsDeleted == true)
            return;

        _journeyRepository.DeleteById(journey);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

    }
}