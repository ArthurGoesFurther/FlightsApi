using Domain.Enums;

namespace Domain.Entities;

public sealed class Flight
{
    public int Id { get; set; }
    public string Origin { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public DateTimeOffset Departure { get; set; }
    public DateTimeOffset Arrival { get; set; }
    public FlightStatus Status { get; set; }
}