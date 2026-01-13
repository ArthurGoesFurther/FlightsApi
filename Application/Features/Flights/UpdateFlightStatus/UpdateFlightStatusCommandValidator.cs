using Domain.Enums;
using FluentValidation;

namespace Application.Features.Flights.UpdateFlightStatus;

public class UpdateFlightStatusCommandValidator : AbstractValidator<UpdateFlightStatusCommand>
{
    public UpdateFlightStatusCommandValidator()
    {
        RuleFor(x => x.FlightId)
            .GreaterThan(0).WithMessage("FlightId must be greater than 0");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Status must be a valid FlightStatus value");
    }
}
