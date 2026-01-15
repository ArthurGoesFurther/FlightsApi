using Application.Features.Auth.GetToken;
using Application.Features.Auth.RegisterUser;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FlightsApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IMediator mediator, ILogger<AuthController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpPost("token")]
    public async Task<IActionResult> GetToken([FromBody] GetTokenRequest request)
    {
        var query = new GetTokenQuery(request.Username, request.Password);
        var result = await _mediator.Send(query);

        if (result == null)
        {
            return Unauthorized(new { message = "Invalid username or password" });
        }

        return Ok(result);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserRequest request)
    {
        var command = new RegisterUserCommand(request.Username, request.Password, request.RoleCode);
        try
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

public record GetTokenRequest(string Username, string Password);
public record RegisterUserRequest(string Username, string Password, string RoleCode);
