using AutoMapper;
using FluentAssertions;
using MediatR;
using NavigationPlatform.JourneyService.Application.Commands.CreateJourney;
using NavigationPlatform.JourneyService.Application.Events;
using NavigationPlatform.JourneyService.Application.Queries.GetJourneyById;
using NavigationPlatform.JourneyService.Domain.Entities;
using NavigationPlatform.JourneyService.Domain.Enums;
using NavigationPlatform.JourneyService.Domain.Interfaces;
using NavigationPlatform.Shared.Identity;
using NavigationPlatform.Shared.Persistance;
using NSubstitute;

namespace NavigationPlatform.JourneyService.UnitTests.Application.Commands.CreateJourney;

public class CreateJourneyCommandHandlerTests
{
    private readonly IJourneyRepository _journeyRepository = Substitute.For<IJourneyRepository>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IMediator _mediator = Substitute.For<IMediator>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();

    private readonly CreateJourneyCommandHandler _sut;

    public CreateJourneyCommandHandlerTests()
    {
        _sut = new CreateJourneyCommandHandler(
            _journeyRepository,
            _currentUserService,
            _mapper,
            _unitOfWork,
            _mediator
        );
    }

    [Fact]
    public async Task Handle_Should_CreateJourney_AndReturnMappedResponse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _currentUserService.GetUserId().Returns(userId);

        var request = new CreateJourneyRequest
        {
            StartLocation = "Rome",
            StartDateTime = DateTime.UtcNow,
            ArrivalLocation = "Vienna",
            ArrivalDateTime = DateTime.UtcNow.AddHours(2),
            TransportationType = TransportationType.Car,
            RouteDistanceKm = 150
        };

        var command = new CreateJourneyCommand(request);

        Journey? capturedJourney = null;
        _journeyRepository.When(r => r.AddJourney(Arg.Any<Journey>()))
                          .Do(ci => capturedJourney = ci.Arg<Journey>());

        var expectedResponse = new JourneyResponse { Id = Guid.NewGuid() };
        _mapper.Map<JourneyResponse>(Arg.Any<Journey>()).Returns(expectedResponse);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _mediator.Received(1).Publish(Arg.Is<JourneyCreatedEventNotification>(n =>
            n.UserId == userId &&
            n.DistanceKm == request.RouteDistanceKm &&
            n.JourneyId != Guid.Empty
        ));

        capturedJourney.Should().NotBeNull();
        capturedJourney!.StartLocation.Should().Be("Rome");
        capturedJourney.ArrivalLocation.Should().Be("Vienna");
        capturedJourney.TransportationType.Should().Be(TransportationType.Car);

        result.Should().Be(expectedResponse);
    }
}
