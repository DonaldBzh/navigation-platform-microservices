using FluentValidation;

namespace NavigationPlatform.JourneyService.Application.Commands.CreateJourney;

public class CreateJourneyCommandValidator : AbstractValidator<CreateJourneyCommand>
{
    public CreateJourneyCommandValidator()
    {
        RuleFor(x => x.Data.StartLocation)
            .NotEmpty().WithMessage("Start location is required")
            .MaximumLength(200).WithMessage("Start location must not exceed 200 characters");

        RuleFor(x => x.Data.ArrivalLocation)
            .NotEmpty().WithMessage("Arrival location is required")
            .MaximumLength(200).WithMessage("Arrival location must not exceed 200 characters");

        RuleFor(x => x.Data.StartDateTime)
            .NotEmpty().WithMessage("Start time is required")
            .LessThan(x => x.Data.ArrivalDateTime).WithMessage("Start time must be before arrival time");

        RuleFor(x => x.Data.ArrivalDateTime)
            .NotEmpty().WithMessage("Arrival time is required")
            .GreaterThan(x => x.Data.StartDateTime).WithMessage("Arrival time must be after start time");

        RuleFor(x => x.Data.TransportationType)
            .IsInEnum().WithMessage("Invalid transportation type");

        RuleFor(x => x.Data.RouteDistanceKm)
            .GreaterThan(0).WithMessage("Distance must be greater than 0")
            .LessThanOrEqualTo(10000).WithMessage("Distance seems unrealistic");
    }
}