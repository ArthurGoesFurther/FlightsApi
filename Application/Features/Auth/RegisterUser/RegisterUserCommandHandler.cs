using Application.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace Application.Features.Auth.RegisterUser;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, RegisterUserResponse>
{
    private readonly IDbContext _context;
    private readonly ILogger<RegisterUserCommandHandler> _logger;

    public RegisterUserCommandHandler(IDbContext context, ILogger<RegisterUserCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<RegisterUserResponse> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        // Check for existing username
        var existing = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username, cancellationToken);
        if (existing != null)
        {
            throw new InvalidOperationException("Username already exists");
        }

        // Find role
        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Code == request.RoleCode, cancellationToken);
        if (role == null)
        {
            // create role if not exists
            role = new Role { Code = request.RoleCode };
            _context.Roles.Add(role);
            await _context.SaveChangesAsync(cancellationToken);
        }

        var hashed = HashPassword(request.Password);

        var user = new User
        {
            Username = request.Username,
            Password = hashed,
            RoleId = role.Id
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User registered: {Username} (Role: {Role})", user.Username, role.Code);

        return new RegisterUserResponse(user.Id, user.Username, role.Code);
    }

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}
