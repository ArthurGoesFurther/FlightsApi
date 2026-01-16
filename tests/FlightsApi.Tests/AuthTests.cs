using Application.Features.Auth.RegisterUser;
using Application.Features.Auth.GetToken;
using Application.Interfaces;
using Domain.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using System.Threading.Tasks;
using Xunit;

namespace FlightsApi.Tests;

public class AuthTests
{
    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("AuthTestsDb")
            .Options;

        return new ApplicationDbContext(options, null, NullLogger<ApplicationDbContext>.Instance);
    }

    [Fact]
    public async Task Register_And_GetToken_Works()
    {
        using var ctx = CreateContext();
        var handler = new RegisterUserCommandHandler(ctx, NullLogger<RegisterUserCommandHandler>.Instance);

        var cmd = new RegisterUserCommand("testuser", "password123", "User");
        var res = await handler.Handle(cmd, default);
        res.Username.Should().Be("testuser");

        var tokenHandler = new GetTokenQueryHandler(ctx, NullLogger<GetTokenQueryHandler>.Instance, new Microsoft.Extensions.Configuration.ConfigurationBuilder().AddInMemoryCollection().Build());
        var q = new GetTokenQuery("testuser", "password123");
        var token = await tokenHandler.Handle(q, default);
        token.Should().NotBeNull();
    }
}
