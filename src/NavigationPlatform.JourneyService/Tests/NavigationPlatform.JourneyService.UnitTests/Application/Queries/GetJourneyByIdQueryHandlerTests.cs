using AutoMapper;
using FluentAssertions;
using NavigationPlatform.JourneyService.Application.Queries.GetJourneyById;
using NavigationPlatform.JourneyService.Domain.Entities;
using NavigationPlatform.JourneyService.Domain.Interfaces;
using NavigationPlatform.Shared.Exceptions;
using NavigationPlatform.Shared.Identity;
using NSubstitute;

namespace NavigationPlatform.JourneyService.UnitTests.Application.Queries;

public class GetJourneyByIdQueryHandlerTests
{
    private readonly IJourneyRepository _journeyRepository = Substitute.For<IJourneyRepository>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private readonly GetJourneyByIdQueryHandler _handler;

    public GetJourneyByIdQueryHandlerTests()
    {
        _handler = new GetJourneyByIdQueryHandler(_journeyRepository, _currentUserService, _mapper);
    }

    [Fact]
    public async Task Handle_Should_Return_JourneyResponse_When_Journey_Exists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var journeyId = Guid.NewGuid();

        var journey = new Journey { Id = journeyId, UserId = userId };
        var journeyResponse = new JourneyResponse { Id = journeyId, UserId = userId };

        _currentUserService.GetUserId().Returns(userId);
        _journeyRepository.GetByIdAndUserIdAsync(journeyId, userId).Returns(journey);
        _mapper.Map<JourneyResponse>(journey).Returns(journeyResponse);

        var query = new GetJourneyByIdQuery { Id = journeyId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(journeyId);
        result.UserId.Should().Be(userId);
    }

    [Fact]

    public async Task Handle_Should_Throw_NotFoundException_When_Journey_Not_Found()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var journeyId = Guid.NewGuid();

        _currentUserService.GetUserId().Returns(userId);
        _journeyRepository.GetByIdAndUserIdAsync(journeyId, userId)
            .Returns((Journey?)null);

        var query = new GetJourneyByIdQuery { Id = journeyId };

        // Act
        var act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Journey not found");

        _mapper.DidNotReceive().Map<JourneyResponse>(Arg.Any<Journey>());
    }
}
