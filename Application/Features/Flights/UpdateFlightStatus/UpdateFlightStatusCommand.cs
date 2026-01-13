using Domain.Entities;
using Domain.Enums;
using MediatR;

namespace Application.Features.Flights.UpdateFlightStatus;

public record UpdateFlightStatusCommand(int FlightId, FlightStatus Status) : IRequest<Flight?>;
