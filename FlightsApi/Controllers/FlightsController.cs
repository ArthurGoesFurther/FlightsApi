using Application.Features.Flights.CreateFlight;
using Application.Features.Flights.GetFlights;
using Application.Features.Flights.UpdateFlightStatus;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace FlightsApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FlightsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<FlightsController> _logger;

    public FlightsController(IMediator mediator, ILogger<FlightsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetFlights([FromQuery] string? origin, [FromQuery] string? destination)
    {
        var query = new GetFlightsQuery(origin, destination);
        var flights = await _mediator.Send(query);
        return Ok(flights);
    }

    [HttpPost]
    [Authorize(Roles = "Moderator")]
    public async Task<IActionResult> CreateFlight([FromBody] CreateFlightRequest request)
    {
        var command = new CreateFlightCommand(
            request.Origin,
            request.Destination,
            request.Departure,
            request.Arrival,
            request.Status);

        var flight = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetFlights), new { id = flight.Id }, flight);
    }

    [HttpPut("{id}/status")]
    [Authorize(Roles = "Moderator")]
    public async Task<IActionResult> UpdateFlightStatus(int id, [FromBody] UpdateFlightStatusRequest request)
    {
        var command = new UpdateFlightStatusCommand(id, request.Status);
        var flight = await _mediator.Send(command);

        if (flight == null)
        {
            return NotFound(new { message = "Flight not found" });
        }

        return Ok(flight);
    }
}

public record CreateFlightRequest(
    string Origin,
    string Destination,
    DateTimeOffset Departure,
    DateTimeOffset Arrival,
    FlightStatus Status);

public record UpdateFlightStatusRequest(FlightStatus Status);
