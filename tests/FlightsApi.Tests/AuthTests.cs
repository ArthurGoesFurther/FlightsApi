using Application.Features.Auth.RegisterUser;
using Application.Features.Auth.GetToken;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.Extensions.Configuration;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using System.Threading.Tasks;
using Xunit;

namespace FlightsApi.Tests;

public class AuthTests
{
    private static Infrastructure.Data.ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<Infrastructure.Data.ApplicationDbContext>()
            .UseInMemoryDatabase("AuthTestsDb")
            .Options;

        return new Infrastructure.Data.ApplicationDbContext(options, null, NullLogger<Infrastructure.Data.ApplicationDbContext>.Instance);
    }

    [Fact]
    public async Task Register_And_GetToken_Works()
    {
        using var ctx = CreateContext();
        var handler = new RegisterUserCommandHandler(ctx, NullLogger<RegisterUserCommandHandler>.Instance);

        var cmd = new RegisterUserCommand("testuser", "password123", "User");
        var res = await handler.Handle(cmd, default);
        res.Username.Should().Be("testuser");

        var config = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
            .AddInMemoryCollection(new System.Collections.Generic.Dictionary<string, string?>
            {
                ["Jwt:Key"] = "8eb039104fed22e5549eed5f86cbfa628eb039104xdarthurkrasava",
                ["Jwt:Issuer"] = "FlightsApi",
                ["Jwt:Audience"] = "FlightsApiClients",
                ["Jwt:ExpiryMinutes"] = "60",
            })
            .Build();

        var tokenHandler = new GetTokenQueryHandler(ctx, NullLogger<GetTokenQueryHandler>.Instance, config);
        var q = new GetTokenQuery("testuser", "password123");
        var token = await tokenHandler.Handle(q, default);
        token.Should().NotBeNull();
    }
}
