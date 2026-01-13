using Domain.Entities;
using Domain.Enums;
using MediatR;

namespace Application.Features.Flights.CreateFlight;

public record CreateFlightCommand(
    string Origin,
    string Destination,
    DateTimeOffset Departure,
    DateTimeOffset Arrival,
    FlightStatus Status) : IRequest<Flight>;
