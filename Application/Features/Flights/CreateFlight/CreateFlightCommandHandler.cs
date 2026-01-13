using Application.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Flights.CreateFlight;

public class CreateFlightCommandHandler : IRequestHandler<CreateFlightCommand, Flight>
{
    private readonly IDbContext _context;
    private readonly ICacheService _cache;
    private readonly ILogger<CreateFlightCommandHandler> _logger;

    public CreateFlightCommandHandler(
        IDbContext context,
        ICacheService cache,
        ILogger<CreateFlightCommandHandler> logger)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
    }

    public async Task<Flight> Handle(CreateFlightCommand request, CancellationToken cancellationToken)
    {
        var flight = new Flight
        {
            Origin = request.Origin,
            Destination = request.Destination,
            Departure = request.Departure,
            Arrival = request.Arrival,
            Status = request.Status
        };

        _context.Flights.Add(flight);
        await _context.SaveChangesAsync(cancellationToken);

        // Invalidate cache
        await _cache.RemoveByPatternAsync("flights_*", cancellationToken);

        _logger.LogInformation("Flight created: ID={FlightId}, Origin={Origin}, Destination={Destination}", 
            flight.Id, flight.Origin, flight.Destination);

        return flight;
    }
}
