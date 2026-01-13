using Domain.Entities;
using MediatR;

namespace Application.Features.Flights.GetFlights;

public record GetFlightsQuery(string? Origin = null, string? Destination = null) : IRequest<List<Flight>>;
