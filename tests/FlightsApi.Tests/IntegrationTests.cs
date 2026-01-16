using Application.Features.Flights.CreateFlight;
using Application.Features.Flights.GetFlights;
using Application.Features.Flights.UpdateFlightStatus;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using System.Threading.Tasks;
using Xunit;
using Infrastructure.Caching;
using Infrastructure.Data;

namespace FlightsApi.Tests;

public class IntegrationTests
{
    private static Infrastructure.Data.ApplicationDbContext CreateContext(string name)
    {
        var options = new DbContextOptionsBuilder<Infrastructure.Data.ApplicationDbContext>()
            .UseInMemoryDatabase(name)
            .Options;

        return new Infrastructure.Data.ApplicationDbContext(options, null, NullLogger<Infrastructure.Data.ApplicationDbContext>.Instance);
    }

    [Fact]
    public async Task CreateFlight_Invalidates_Cache_Then_GetFlights_Reads_Db()
    {
        using var ctx = CreateContext("intdb1");
        var memory = new MemoryCache(new MemoryCacheOptions());
        var cache = new MemoryCacheService(memory);

        var createHandler = new CreateFlightCommandHandler(ctx, cache, NullLogger<CreateFlightCommandHandler>.Instance);
        var getHandler = new GetFlightsQueryHandler(ctx, cache, NullLogger<GetFlightsQueryHandler>.Instance);

        // initially empty
        var list1 = await getHandler.Handle(new GetFlightsQuery(null, null), default);
        list1.Should().BeEmpty();

        // create flight
        var flight = await createHandler.Handle(new CreateFlightCommand("O", "D", System.DateTimeOffset.UtcNow, System.DateTimeOffset.UtcNow.AddHours(1), Domain.Enums.FlightStatus.InTime), default);
        flight.Should().NotBeNull();

        // cache should have been invalidated and DB should return the new flight
        var list2 = await getHandler.Handle(new GetFlightsQuery(null, null), default);
        list2.Should().HaveCount(1);

        // update flight status
        var updateHandler = new UpdateFlightStatusCommandHandler(ctx, cache, NullLogger<UpdateFlightStatusCommandHandler>.Instance);
        var updated = await updateHandler.Handle(new UpdateFlightStatusCommand(flight.Id, Domain.Enums.FlightStatus.Delayed), default);
        updated.Should().NotBeNull();

        var list3 = await getHandler.Handle(new GetFlightsQuery(null, null), default);
        list3.Should().HaveCount(1);
        list3[0].Status.Should().Be(Domain.Enums.FlightStatus.Delayed);
    }
}
