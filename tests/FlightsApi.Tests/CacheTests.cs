using Application.Interfaces;
using Infrastructure.Caching;
using Microsoft.Extensions.Caching.Memory;
using FluentAssertions;
using Xunit;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace FlightsApi.Tests;

public class CacheTests
{
    [Fact]
    public async Task GetFlights_Cache_Set_And_RemoveByPattern_Works()
    {
        var memory = new MemoryCache(new MemoryCacheOptions());
        var cache = new MemoryCacheService(memory);

        var key1 = "flights__";
        var key2 = "flights_origin1_dest1";
        var data = new List<string> { "a", "b" };

        await cache.SetAsync(key1, data);
        await cache.SetAsync(key2, data);

        var got = await cache.GetAsync<List<string>>(key1);
        got.Should().NotBeNull();

        await cache.RemoveByPatternAsync("flights_*");

        var got1 = await cache.GetAsync<List<string>>(key1);
        var got2 = await cache.GetAsync<List<string>>(key2);

        got1.Should().BeNull();
        got2.Should().BeNull();
    }
}
