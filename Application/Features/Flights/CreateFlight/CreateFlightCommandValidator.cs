using Domain.Enums;
using FluentValidation;

namespace Application.Features.Flights.CreateFlight;

public class CreateFlightCommandValidator : AbstractValidator<CreateFlightCommand>
{
    public CreateFlightCommandValidator()
    {
        RuleFor(x => x.Origin)
            .NotEmpty().WithMessage("Origin is required")
            .MaximumLength(256).WithMessage("Origin must not exceed 256 characters");

        RuleFor(x => x.Destination)
            .NotEmpty().WithMessage("Destination is required")
            .MaximumLength(256).WithMessage("Destination must not exceed 256 characters");

        RuleFor(x => x.Departure)
            .NotEmpty().WithMessage("Departure is required");

        RuleFor(x => x.Arrival)
            .NotEmpty().WithMessage("Arrival is required")
            .GreaterThan(x => x.Departure).WithMessage("Arrival must be after Departure");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Status must be a valid FlightStatus value");
    }
}
