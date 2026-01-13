using Application.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Flights.UpdateFlightStatus;

public class UpdateFlightStatusCommandHandler : IRequestHandler<UpdateFlightStatusCommand, Flight?>
{
    private readonly IDbContext _context;
    private readonly ICacheService _cache;
    private readonly ILogger<UpdateFlightStatusCommandHandler> _logger;

    public UpdateFlightStatusCommandHandler(
        IDbContext context,
        ICacheService cache,
        ILogger<UpdateFlightStatusCommandHandler> logger)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
    }

    public async Task<Flight?> Handle(UpdateFlightStatusCommand request, CancellationToken cancellationToken)
    {
        var flight = await _context.Flights
            .FirstOrDefaultAsync(f => f.Id == request.FlightId, cancellationToken);

        if (flight == null)
        {
            _logger.LogWarning("Flight not found: ID={FlightId}", request.FlightId);
            return null;
        }

        flight.Status = request.Status;
        await _context.SaveChangesAsync(cancellationToken);

        // Invalidate cache
        await _cache.RemoveByPatternAsync("flights_*", cancellationToken);

        _logger.LogInformation("Flight status updated: ID={FlightId}, Status={Status}", 
            flight.Id, flight.Status);

        return flight;
    }
}
