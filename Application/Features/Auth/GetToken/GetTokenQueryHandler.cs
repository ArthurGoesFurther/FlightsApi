using Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace Application.Features.Auth.GetToken;

public class GetTokenQueryHandler : IRequestHandler<GetTokenQuery, GetTokenResponse?>
{
    private readonly IDbContext _context;
    private readonly ILogger<GetTokenQueryHandler> _logger;
    private readonly IConfiguration _configuration;

    public GetTokenQueryHandler(IDbContext context, ILogger<GetTokenQueryHandler> logger, IConfiguration configuration)
    {
        _context = context;
        _logger = logger;
        _configuration = configuration;
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

        var isValid = Application.Common.PasswordHasher.VerifyHashedPassword(user.Password, request.Password);
        if (!isValid)
        {
            _logger.LogWarning("Invalid password attempt for user: {Username}", request.Username);
            return null;
        }

        // Generate JWT token
        var key = _configuration["Jwt:Key"] ?? string.Empty;
        if (string.IsNullOrEmpty(key))
        {
            _logger.LogError("JWT Key is not configured");
            return null;
        }

        var issuer = _configuration["Jwt:Issuer"] ?? "FlightsApi";
        var audience = _configuration["Jwt:Audience"] ?? "FlightsApiClients";
        var expiryMinutes = int.TryParse(_configuration["Jwt:ExpiryMinutes"], out var m) ? m : 60;

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
        };

        if (!string.IsNullOrEmpty(user.Role?.Code))
        {
            claims.Add(new Claim(ClaimTypes.Role, user.Role!.Code));
        }

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: credentials);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        _logger.LogInformation("User {Username} authenticated successfully", request.Username);
        return new GetTokenResponse(tokenString);
    }

    // password verification moved to PasswordHasher
}
