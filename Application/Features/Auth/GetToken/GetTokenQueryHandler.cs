using Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace Application.Features.Auth.GetToken;

public class GetTokenQueryHandler : IRequestHandler<GetTokenQuery, GetTokenResponse?>
{
    private readonly IDbContext _context;
    private readonly ILogger<GetTokenQueryHandler> _logger;

    public GetTokenQueryHandler(IDbContext context, ILogger<GetTokenQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<GetTokenResponse?> Handle(GetTokenQuery request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Username == request.Username, cancellationToken);

        if (user == null)
        {
            _logger.LogWarning("Login attempt with non-existent username: {Username}", request.Username);
            return null;
        }

        var hashedPassword = HashPassword(request.Password);
        if (user.Password != hashedPassword)
        {
            _logger.LogWarning("Invalid password attempt for user: {Username}", request.Username);
            return null;
        }

        // Simple token generation (in production use JWT)
        var token = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{user.Username}:{user.RoleId}:{DateTime.UtcNow:O}"));

        _logger.LogInformation("User {Username} authenticated successfully", request.Username);
        return new GetTokenResponse(token);
    }

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}
