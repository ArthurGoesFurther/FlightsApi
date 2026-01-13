using Application.Features.Flights.GetFlights;
using Application.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Flights.GetFlights;

public class GetFlightsQueryHandler : IRequestHandler<GetFlightsQuery, List<Flight>>
{
    private readonly IDbContext _context;
    private readonly ICacheService _cache;
    private readonly ILogger<GetFlightsQueryHandler> _logger;

    public GetFlightsQueryHandler(
        IDbContext context,
        ICacheService cache,
        ILogger<GetFlightsQueryHandler> logger)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
    }

    public async Task<List<Flight>> Handle(GetFlightsQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"flights_{request.Origin}_{request.Destination}";
        var cached = await _cache.GetAsync<List<Flight>>(cacheKey, cancellationToken);
        
        if (cached != null)
        {
            _logger.LogInformation("Retrieved flights from cache");
            return cached;
        }

        var query = _context.Flights.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Origin))
        {
            query = query.Where(f => f.Origin == request.Origin);
        }

        if (!string.IsNullOrWhiteSpace(request.Destination))
        {
            query = query.Where(f => f.Destination == request.Destination);
        }

        var flights = await query
            .OrderBy(f => f.Arrival)
            .ToListAsync(cancellationToken);

        await _cache.SetAsync(cacheKey, flights, TimeSpan.FromMinutes(10), cancellationToken);
        _logger.LogInformation("Retrieved {Count} flights from database and cached", flights.Count);

        return flights;
    }
}
