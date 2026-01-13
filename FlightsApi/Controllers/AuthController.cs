using Application.Features.Auth.GetToken;
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
}

public record GetTokenRequest(string Username, string Password);
